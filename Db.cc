using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

# ----------------------------------------------------
# 1. PLUGIN CONTRACT (CORE ABSTRACTION)
# ----------------------------------------------------

public interface IUserDataSource
{
    Task<IEnumerable<User>> GetUsersAsync();
}

# ----------------------------------------------------
# 2. DB SOURCE
# ----------------------------------------------------

public class DbUserSource : IUserDataSource
{
    private readonly TestDataConfig _config;

    public DbUserSource(IOptions<TestDataConfig> config)
    {
        _config = config.Value;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var list = new List<User>();

        using var conn = new SqlConnection(_config.ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Id, Name FROM {_config.Table}";

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return list;
    }
}

# ----------------------------------------------------
# 3. CSV SOURCE
# ----------------------------------------------------

public class CsvUserSource : IUserDataSource
{
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
            .Select(l =>
            {
                var parts = l.Split(',');
                return new User
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1]
                };
            });
    }
}

# ----------------------------------------------------
# 4. JSON SOURCE
# ----------------------------------------------------

public class JsonUserSource : IUserDataSource
{
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

# ----------------------------------------------------
# 5. API SOURCE
# ----------------------------------------------------

public class ApiUserSource : IUserDataSource
{
    private readonly TestDataConfig _config;
    private readonly HttpClient _http;

    public ApiUserSource(IOptions<TestDataConfig> config, HttpClient http)
    {
        _config = config.Value;
        _http = http;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var json = await _http.GetStringAsync(_config.ApiUrl);

        return JsonSerializer.Deserialize<List<User>>(json)
               ?? new List<User>();
    }
}

# ----------------------------------------------------
# 6. AGGREGATOR (THE PIPELINE ENGINE)
# ----------------------------------------------------

public class UnifiedUserDataProvider
{
    private readonly IEnumerable<IUserDataSource> _sources;

    public UnifiedUserDataProvider(IEnumerable<IUserDataSource> sources)
    {
        _sources = sources;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var results = new List<User>();

        foreach (var source in _sources)
        {
            var data = await source.GetUsersAsync();
            results.AddRange(data);
        }

        return results;
    }
}

# ----------------------------------------------------
# 7. TEST HOST (PLUGIN REGISTRY)
# ----------------------------------------------------

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

        // register plugin sources
        services.AddTransient<IUserDataSource, DbUserSource>();
        services.AddTransient<IUserDataSource, CsvUserSource>();
        services.AddTransient<IUserDataSource, JsonUserSource>();
        services.AddHttpClient<IUserDataSource, ApiUserSource>();

        services.AddTransient<UnifiedUserDataProvider>();

        Services = services.BuildServiceProvider();
    }
}

# ----------------------------------------------------
# 8. TEST DATA EXPANSION (FINAL PIPELINE OUTPUT)
# ----------------------------------------------------

public static class UserTestData
{
    public static IEnumerable<object[]> Users()
    {
        var provider = TestHost.Services.GetRequiredService<UnifiedUserDataProvider>();

        var users = provider.GetAllUsersAsync().Result;

        foreach (var user in users)
        {
            yield return new object[] { user };
        }
    }
}

# ----------------------------------------------------
# 9. TEST EXECUTION
# ----------------------------------------------------

public class UserTests
{
    [Theory]
    [MemberData(nameof(UserTestData.Users), MemberType = typeof(UserTestData))]
    public void ValidateUser(User user)
    {
        Assert.NotNull(user);
        Assert.False(string.IsNullOrWhiteSpace(user.Name));
        Assert.True(user.Id > 0);
    }
}
