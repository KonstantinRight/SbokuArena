
using Sandbox.Sboku.Arena;
using SWB.Base;
using System;
using System.Collections.Generic;

namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public int MaxHealth { get; set; } = 100;
	[Property] public float DamageThrottle { get; set; } = 0.3f;
	[Property] public int MaxDamageClamp { get; set; } = 15;
	[Sync] public int Health { get; set; } = 100;
	[Sync] public int Kills { get; set; }
	[Sync] public int Deaths { get; set; }
	[Sync] public bool GodMode { get; set; }

    public bool IsAlive => Health > 0;
	
	private Dictionary<string, DamageEntry> dmgTable = new();
	private sealed class DamageEntry
    {
        public Shared.DamageInfo Info { get; }
        public int Damage { get; set; }
        public float Since { get; set; }

        public DamageEntry(Shared.DamageInfo info)
        {
            Info = info;
            Damage = 0;
            Since = 0f;
        }
    }

	private void HandleDamage()
	{
		var remove = new List<string>();

		foreach (var entry in dmgTable)
		{
			entry.Value.Since += Time.Delta;
			if (entry.Value.Since > DamageThrottle)
			{
				InflictDamage(entry.Value.Info, entry.Value.Damage);
				remove.Add(entry.Key);
			}
		}

		remove.ForEach(x => dmgTable.Remove(x));
    }

    private void InflictDamage(Shared.DamageInfo info, int dmg)
	{
        Health -= dmg;
        // Flinch
        var weapon = WeaponRegistry.Instance.Get(info.Inflictor);
        if (weapon is not null)
            DoHitFlinch(weapon.Primary.HitFlinch);

        if (Health <= 0)
            OnDeath(info);
    }

	[Rpc.Broadcast]
	public virtual void TakeDamage( Shared.DamageInfo info )
	{
		if ( !IsValid || IsProxy || !IsAlive || GodMode )
			return;

		if ( Array.Exists( info.Tags, tag => tag == "head" ) )
			info.Damage *= 2;

		float dmgMultiplier = 1;
		var attacker = Scene.Directory.FindByGuid(info.AttackerId);
		if (attacker != null && attacker.IsValid)
		{
			dmgMultiplier = attacker.GetComponent<UpgradeHolder>().DamageMultiplier;
		}

        var dmg = (int)(MathF.Round(info.Damage * dmgMultiplier));
		if (!dmgTable.ContainsKey(info.Inflictor))
		{
			dmgTable.Add(info.Inflictor, new(info));
		}
		dmgTable[info.Inflictor].Damage = Math.Clamp(dmgTable[info.Inflictor].Damage + dmg, 1, (int)MathF.Round(MaxDamageClamp * GetComponent<UpgradeHolder>().ArmorMultiplier));
    }

    [Rpc.Broadcast]
    public virtual void TakeDamage(int damage)
    {
        if (!IsValid || IsProxy || !IsAlive || GodMode)
            return;

        Health -= damage;
    }
}
