using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#region MODEL

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

#endregion

#region PEER NODE

public class PeerNode
{
    public string NodeId { get; }
    public string Endpoint { get; }

    private readonly HttpClient _http;

    public PeerNode(string id, string endpoint, HttpClient http)
    {
        NodeId = id;
        Endpoint = endpoint;
        _http = http;
    }

    public async Task<List<User>> FetchAsync()
    {
        try
        {
            var json = await _http.GetStringAsync($"{Endpoint}/users");
            return JsonSerializer.Deserialize<List<User>>(json)
                   ?? new List<User>();
        }
        catch
        {
            return new List<User>();
        }
    }
}

#endregion

#region REGISTRY

public class PeerRegistry
{
    private readonly List<PeerNode> _nodes = new();

    public void Register(PeerNode node) => _nodes.Add(node);
    public IEnumerable<PeerNode> All() => _nodes;
}

#endregion

#region CRYPTO + MANIFEST

public class CryptoEngine
{
    public string Hash(object data)
    {
        var json = JsonSerializer.Serialize(data);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }

    public string Sign(string data, string key)
    {
        var bytes = Encoding.UTF8.GetBytes(data + key);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    public bool Verify(string data, string signature, string key)
        => Sign(data, key) == signature;
}

public class NodeManifest
{
    public string NodeId { get; set; } = "";
    public string DataHash { get; set; } = "";
    public string Signature { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

#endregion

#region DISTRIBUTED ENGINE

public class DistributedEngine
{
    private readonly PeerRegistry _registry;

    public DistributedEngine(PeerRegistry registry)
    {
        _registry = registry;
    }

    public async Task<(List<List<User>> data, List<NodeManifest> manifests)> CollectAsync()
    {
        var crypto = new CryptoEngine();

        var tasks = _registry.All().Select(async node =>
        {
            var data = await node.FetchAsync();

            var hash = crypto.Hash(data);

            var manifest = new NodeManifest
            {
                NodeId = node.NodeId,
                DataHash = hash,
                Signature = crypto.Sign(hash, node.NodeId),
                Timestamp = DateTime.UtcNow
            };

            return (data, manifest);
        });

        var results = await Task.WhenAll(tasks);

        return (
            results.Select(x => x.data).ToList(),
            results.Select(x => x.manifest).ToList()
        );
    }
}

#endregion

#region CONSENSUS ENGINE

public class ConsensusEngine
{
    public List<User> Resolve(List<List<User>> nodes)
    {
        return nodes
            .SelectMany(x => x)
            .GroupBy(u => u.Id)
            .Select(g =>
            {
                var bestName = g
                    .GroupBy(x => x.Name)
                    .OrderByDescending(x => x.Count())
                    .First()
                    .Key;

                return new User
                {
                    Id = g.Key,
                    Name = bestName
                };
            })
            .ToList();
    }
}

#endregion

#region AI ENGINE

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

#endregion

#region SELF HEALING

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

#region MEMORY (LEDGER)

public class MemoryStore
{
    private readonly Dictionary<string, List<User>> _store = new();

    public string Save(List<User> data)
    {
        var json = JsonSerializer.Serialize(data);
        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(json))
        );

        _store[hash] = data;
        return hash;
    }

    public List<User>? Load(string hash)
        => _store.TryGetValue(hash, out var data) ? data : null;
}

#endregion

#region DECENTRALIZED KERNEL (FINAL BRAIN)

public class DecentralizedKernel
{
    private readonly DistributedEngine _distributed;
    private readonly ConsensusEngine _consensus;
    private readonly SelfHealingEngine _heal;
    private readonly AIEngine _ai;
    private readonly MemoryStore _memory;

    public DecentralizedKernel(
        DistributedEngine distributed,
        ConsensusEngine consensus,
        SelfHealingEngine heal,
        AIEngine ai,
        MemoryStore memory)
    {
        _distributed = distributed;
        _consensus = consensus;
        _heal = heal;
        _ai = ai;
        _memory = memory;
    }

    public async Task<string> RunAsync()
    {
        // 1. collect from all peers
        var (data, manifests) = await _distributed.CollectAsync();

        // 2. consensus merge (truth formation)
        var merged = _consensus.Resolve(data);

        // 3. self-heal corrupted entries
        var healed = _heal.Repair(merged);

        // 4. AI refinement layer
        var final = _ai.Resolve(healed);

        // 5. persist snapshot
        return _memory.Save(final);
    }
}

#endregion

#region OS CORE

public class Web4DecentralizedOS
{
    private readonly DecentralizedKernel _kernel;
    private readonly MemoryStore _memory;

    public Web4DecentralizedOS(DecentralizedKernel kernel, MemoryStore memory)
    {
        _kernel = kernel;
        _memory = memory;
    }

    public Task<string> BootAsync()
        => _kernel.RunAsync();

    public List<User>? Restore(string snapshot)
        => _memory.Load(snapshot);
}

#endregion

#region BOOTSTRAP

public static class Web4Host
{
    public static Web4DecentralizedOS Build()
    {
        var http = new HttpClient();

        var registry = new PeerRegistry();

        registry.Register(new PeerNode("node-1", "https://node1.local", http));
        registry.Register(new PeerNode("node-2", "https://node2.local", http));
        registry.Register(new PeerNode("node-3", "https://node3.local", http));

        var distributed = new DistributedEngine(registry);

        var kernel = new DecentralizedKernel(
            distributed,
            new ConsensusEngine(),
            new SelfHealingEngine(),
            new AIEngine(),
            new MemoryStore()
        );

        return new Web4DecentralizedOS(kernel, new MemoryStore());
    }
}

#endregion
