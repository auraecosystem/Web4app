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
    public string JsonPath { get; set; } = "";
    public string ApiUrl { get; set; } = "";
}

#endregion

#region PLUGINS (DATA SOURCES)

public interface IUserDataSource
{
    string Name { get; }
    int Priority { get; }
    Task<IEnumerable<User>> GetUsersAsync();
}

#endregion

#region SOURCES

public class DbSource : IUserDataSource
{
    public string Name => "DB";
    public int Priority => 1;

    private readonly TestDataConfig _cfg;

    public DbSource(IOptions<TestDataConfig> cfg) => _cfg = cfg.Value;

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var list = new List<User>();

        using var conn = new SqlConnection(_cfg.ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Id, Name FROM {_cfg.Table}";

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

public class CsvSource : IUserDataSource
{
    public string Name => "CSV";
    public int Priority => 3;

    private readonly TestDataConfig _cfg;

    public CsvSource(IOptions<TestDataConfig> cfg) => _cfg = cfg.Value;

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var lines = await File.ReadAllLinesAsync(_cfg.CsvPath);

        return lines.Skip(1)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x =>
            {
                var p = x.Split(',');
                return new User
                {
                    Id = int.Parse(p[0]),
                    Name = p[1]
                };
            });
    }
}

public class JsonSource : IUserDataSource
{
    public string Name => "JSON";
    public int Priority => 2;

    private readonly TestDataConfig _cfg;

    public JsonSource(IOptions<TestDataConfig> cfg) => _cfg = cfg.Value;

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var json = await File.ReadAllTextAsync(_cfg.JsonPath);

        return JsonSerializer.Deserialize<List<User>>(json)
               ?? new List<User>();
    }
}

public class ApiSource : IUserDataSource
{
    public string Name => "API";
    public int Priority => 4;

    private readonly HttpClient _http;
    private readonly TestDataConfig _cfg;

    public ApiSource(IOptions<TestDataConfig> cfg, HttpClient http)
    {
        _cfg = cfg.Value;
        _http = http;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        try
        {
            var json = await _http.GetStringAsync(_cfg.ApiUrl);
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

#region DISTRIBUTED NODE SYSTEM

public interface IDataNode
{
    string NodeId { get; }
    Task<IEnumerable<User>> FetchAsync();
}

public class HttpNode : IDataNode
{
    public string NodeId { get; }
    private readonly string _endpoint;
    private readonly HttpClient _http;

    public HttpNode(string id, string endpoint, HttpClient http)
    {
        NodeId = id;
        _endpoint = endpoint;
        _http = http;
    }

    public async Task<IEnumerable<User>> FetchAsync()
    {
        try
        {
            var json = await _http.GetStringAsync($"{_endpoint}/users");
            return JsonSerializer.Deserialize<List<User>>(json)
                   ?? new List<User>();
        }
        catch
        {
            return Enumerable.Empty<User>();
        }
    }
}

public class NodeRegistry
{
    private readonly List<IDataNode> _nodes = new();
    public void Register(IDataNode node) => _nodes.Add(node);
    public IEnumerable<IDataNode> All() => _nodes;
}

#endregion

#region MEMORY + CACHE

public class MemoryStore
{
    private readonly Dictionary<string, List<User>> _store = new();

    public string Save(List<User> data)
    {
        var hash = Hash(data);
        _store[hash] = data;
        return hash;
    }

    public List<User>? Load(string hash)
        => _store.TryGetValue(hash, out var d) ? d : null;

    private string Hash(List<User> data)
    {
        var json = JsonSerializer.Serialize(data);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }
}

#endregion

#region AI + SELF HEALING

public class AIEngine
{
    public List<User> Resolve(List<User> users)
    {
        return users
            .GroupBy(u => u.Id)
            .Select(g => g.OrderByDescending(Score).First())
            .ToList();
    }

    private int Score(User u)
        => (u.Id > 0 ? 10 : 0) + (!string.IsNullOrWhiteSpace(u.Name) ? 10 : 0);
}

public class SelfHealingEngine
{
    public List<User> Repair(List<User> users)
    {
        return users.Select(u => new User
        {
            Id = u.Id <= 0 ? Random.Shared.Next(1000, 999999) : u.Id,
            Name = string.IsNullOrWhiteSpace(u.Name) ? "Unknown" : u.Name
        }).ToList();
    }
}

#endregion

#region DISTRIBUTED EXECUTION

public class DistributedEngine
{
    private readonly NodeRegistry _registry;

    public DistributedEngine(NodeRegistry registry)
        => _registry = registry;

    public async Task<List<User>> ExecuteAsync()
    {
        var tasks = _registry.All().Select(n => n.FetchAsync());
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(x => x).ToList();
    }
}

#endregion

#region WEB4 KERNEL

public class Web4Kernel
{
    private readonly DistributedEngine _distributed;
    private readonly SelfHealingEngine _heal;
    private readonly AIEngine _ai;

    public Web4Kernel(
        DistributedEngine d,
        SelfHealingEngine h,
        AIEngine a)
    {
        _distributed = d;
        _heal = h;
        _ai = a;
    }

    public async Task<List<User>> RunAsync()
    {
        var raw = await _distributed.ExecuteAsync();
        var fixedData = _heal.Repair(raw);
        return _ai.Resolve(fixedData);
    }
}

#endregion

#region OS CORE

public class Web4OS
{
    private readonly Web4Kernel _kernel;
    private readonly MemoryStore _memory;

    public Web4OS(Web4Kernel kernel, MemoryStore memory)
    {
        _kernel = kernel;
        _memory = memory;
    }

    public async Task<string> BootAsync()
    {
        var data = await _kernel.RunAsync();
        return _memory.Save(data);
    }

    public List<User>? Restore(string snapshot)
        => _memory.Load(snapshot);
}

#endregion

#region BOOTSTRAP

public static class Web4Host
{
    public static IServiceProvider Services { get; }

    static Web4Host()
    {
        var services = new ServiceCollection();
        var http = new HttpClient();

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        services.Configure<TestDataConfig>(cfg);

        // sources
        services.AddTransient<IUserDataSource, DbSource>();
        services.AddTransient<IUserDataSource, CsvSource>();
        services.AddTransient<IUserDataSource, JsonSource>();
        services.AddHttpClient<IUserDataSource, ApiSource>();

        // distributed
        var registry = new NodeRegistry();
        registry.Register(new HttpNode("node1", "https://node1.local", http));
        registry.Register(new HttpNode("node2", "https://node2.local", http));

        services.AddSingleton(registry);
        services.AddSingleton<DistributedEngine>();

        // core
        services.AddSingleton<SelfHealingEngine>();
        services.AddSingleton<AIEngine>();
        services.AddSingleton<Web4Kernel>();
        services.AddSingleton<MemoryStore>();
        services.AddSingleton<Web4OS>();

        Services = services.BuildServiceProvider();
    }
}

#endregion
