using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

#region MODEL

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

#endregion

#region CONFIG

public class TestDataConfig
{
    public string ConnectionString { get; set; } = "";
    public string Table { get; set; } = "";
    public string CsvPath { get; set; } = "";
    public string ApiUrl { get; set; } = "";
    public string JsonPath { get; set; } = "";
}

#endregion

#region PLUGIN CONTRACT

public interface IUserDataSource
{
    string Name { get; }
    int Priority { get; }

    Task<IEnumerable<User>> GetUsersAsync();
}

#endregion

#region DB SOURCE

public class DbUserSource : IUserDataSource
{
    public string Name => "Database";
    public int Priority => 1;

    private readonly TestDataConfig _config;

    public DbUserSource(IOptions<TestDataConfig> config)
    {
        _config = config.Value;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var result = new List<User>();

        using var conn = new SqlConnection(_config.ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Id, Name FROM {_config.Table}";

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return result;
    }
}

#endregion

#region CSV SOURCE

public class CsvUserSource : IUserDataSource
{
    public string Name => "CSV";
    public int Priority => 3;

    private readonly TestDataConfig _config;

    public CsvUserSource(IOptions<TestDataConfig> config)
    {
        _config = config.Value;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var lines = await File.ReadAllLinesAsync(_config.CsvPath);

        return lines
            .Skip(1)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(line =>
            {
                var parts = line.Split(',');

                return new User
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1]
                };
            });
    }
}

#endregion

#region JSON SOURCE

public class JsonUserSource : IUserDataSource
{
    public string Name => "JSON";
    public int Priority => 2;

    private readonly TestDataConfig _config;

    public JsonUserSource(IOptions<TestDataConfig> config)
    {
        _config = config.Value;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var json = await File.ReadAllTextAsync(_config.JsonPath);

        return JsonSerializer.Deserialize<List<User>>(json)
               ?? new List<User>();
    }
}

#endregion

#region API SOURCE (RESILIENT)

public class ApiUserSource : IUserDataSource
{
    public string Name => "API";
    public int Priority => 4;

    private readonly TestDataConfig _config;
    private readonly HttpClient _http;

    public ApiUserSource(IOptions<TestDataConfig> config, HttpClient http)
    {
        _config = config.Value;
        _http = http;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        try
        {
            _http.Timeout = TimeSpan.FromSeconds(5);

            var json = await _http.GetStringAsync(_config.ApiUrl);

            return JsonSerializer.Deserialize<List<User>>(json)
                   ?? new List<User>();
        }
        catch
        {
            return Enumerable.Empty<User>();
        }
    }
}

#endregion

#region FUSION ENGINE

public class UserFusionEngine
{
    public List<User> Merge(IEnumerable<User> users)
    {
        return users
            .GroupBy(u => u.Id)
            .Select(g => g.First())
            .ToList();
    }
}

#endregion

#region CACHE

public class DatasetCache
{
    private readonly Dictionary<string, List<User>> _cache = new();

    public string Store(List<User> users)
    {
        var hash = ComputeHash(users);

        if (!_cache.ContainsKey(hash))
            _cache[hash] = users;

        return hash;
    }

    public List<User>? Get(string hash)
        => _cache.TryGetValue(hash, out var data) ? data : null;

    private string ComputeHash(List<User> users)
    {
        var json = JsonSerializer.Serialize(users);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }
}

#endregion

#region AUDIT LEDGER

public class AuditEvent
{
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = "";
    public string DatasetHash { get; set; } = "";
}

public class AuditLedger
{
    private readonly List<AuditEvent> _events = new();

    public void Record(string source, string hash)
    {
        _events.Add(new AuditEvent
        {
            Timestamp = DateTime.UtcNow,
            Source = source,
            DatasetHash = hash
        });
    }

    public IEnumerable<AuditEvent> GetAll() => _events;
}

#endregion

#region EXECUTION ENGINE

public class DataMeshEngine
{
    private readonly IEnumerable<IUserDataSource> _sources;
    private readonly UserFusionEngine _fusion;
    private readonly DatasetCache _cache;
    private readonly AuditLedger _ledger;

    public DataMeshEngine(
        IEnumerable<IUserDataSource> sources,
        UserFusionEngine fusion,
        DatasetCache cache,
        AuditLedger ledger)
    {
        _sources = sources;
        _fusion = fusion;
        _cache = cache;
        _ledger = ledger;
    }

    public async Task<List<User>> ExecuteAsync()
    {
        var tasks = _sources
            .OrderBy(s => s.Priority)
            .Select(async source =>
            {
                var data = await source.GetUsersAsync();

                var list = data.ToList();

                var hash = _cache.Store(list);
                _ledger.Record(source.Name, hash);

                return list;
            });

        var results = await Task.WhenAll(tasks);

        var merged = _fusion.Merge(results.SelectMany(x => x));

        return merged;
    }
}

#endregion

#region TEST HOST

public static class TestHost
{
    public static IServiceProvider Services { get; }

    static TestHost()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TestData:ConnectionString"] = "Server=localhost;Database=TestDb;",
                ["TestData:Table"] = "Users",
                ["TestData:CsvPath"] = "users.csv",
                ["TestData:JsonPath"] = "users.json",
                ["TestData:ApiUrl"] = "https://example.com/users"
            })
            .Build();

        var services = new ServiceCollection();

        services.Configure<TestDataConfig>(config.GetSection("TestData"));

        services.AddTransient<IUserDataSource, DbUserSource>();
        services.AddTransient<IUserDataSource, CsvUserSource>();
        services.AddTransient<IUserDataSource, JsonUserSource>();
        services.AddHttpClient<IUserDataSource, ApiUserSource>();

        services.AddSingleton<UserFusionEngine>();
        services.AddSingleton<DatasetCache>();
        services.AddSingleton<AuditLedger>();
        services.AddTransient<DataMeshEngine>();

        Services = services.BuildServiceProvider();
    }
}

#endregion

#region TEST DATA

public static class UserTestData
{
    public static async Task<IEnumerable<object[]>> Users()
    {
        var engine = TestHost.Services.GetRequiredService<DataMeshEngine>();

        var users = await engine.ExecuteAsync();

        return users.Select(u => new object[] { u });
    }
}

#endregion

#region TEST

public class UserTests
{
    [Theory]
    [MemberData(nameof(UserTestData.Users), MemberType = typeof(UserTestData))]
    public void ValidateUser(User user)
    {
        Assert.NotNull(user);
        Assert.True(user.Id > 0);
        Assert.False(string.IsNullOrWhiteSpace(user.Name));
    }
}

#endregion
