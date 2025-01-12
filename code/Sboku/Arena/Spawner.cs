using System.Linq;
using System.Collections.Generic;
using SWB.Demo;

namespace Sandbox.Sboku.Arena;
[Group("Sboku Arena")]
internal class Spawner : Component, Component.ITriggerListener
{
    [Property]
    public PrefabScene Prefab { get; set; }
    [Property]
    public GameObject SpawnPoint { get; set; }
    [Property]
    [Range(1f, 100f, step: 1f)]
    public float SpawnInterval { get; set; } = 5f;
    [Property]
    public float DamageInterval { get; set; } = 0.5f;

    private HashSet<Collider> inside = new();
    private int spawnCounter = 0;
    private TimeSince? lastSpawn = null;
    private TimeSince sinceDamage = new();

    public void RequestSpawn() => spawnCounter++;

    private void Spawn()
    {
        if (inside.Any()) return;

        spawnCounter--;

        var bot = Prefab.Clone();
        var upgrades = bot.GetComponent<UpgradeHolder>();
        upgrades.FreePoints = 0; // TODO:
        upgrades.DistributeRandomly();
        bot.WorldPosition = SpawnPoint.WorldPosition;

        if (lastSpawn != null)
            lastSpawn = 0f;
        else
            lastSpawn = null;
    }

    protected override void DrawGizmos()
    {
        if (SpawnPoint == null) return;

        Gizmo.Draw.Color = Color.Orange;
        Gizmo.Draw.SolidSphere(SpawnPoint.LocalPosition, 1);
    }

    protected override void OnAwake()
    {
        if (inside.Any())
        {
            Log.Warning("There is something inside " + GameObject);
        }
    }

    protected override void OnFixedUpdate()
    {
        if (spawnCounter > 0)
        {
            if (lastSpawn == null || lastSpawn > SpawnInterval)
            {
                Spawn();
            }
        }
        else if (lastSpawn != null)
        {
            lastSpawn = null;
        }

        bool damageFlag = false;
        var players = inside.Where(x => x.GameObject.GetComponentInParent<DemoPlayer>() != null)
                            .Select(x => x.GameObject.GetComponentInParent<DemoPlayer>());

        foreach (var ply in players)
        {
            if (sinceDamage > DamageInterval)
            {
                ply.TakeDamage(5);
                damageFlag = true;
            }
        }

        if (damageFlag)
            sinceDamage = 0f;
    }

    public void OnTriggerEnter(Collider other)
        => inside.Add(other);
    public void OnTriggerExit(Collider other)
        => inside.Remove(other);
}
