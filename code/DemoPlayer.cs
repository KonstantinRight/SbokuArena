using Sandbox.Sboku.Arena;
using Sandbox.Sboku.UI;
using SWB.Base;
using SWB.HUD;
using SWB.Player;
using System.Linq;

namespace SWB.Demo;

[Group( "SWB" )]
[Title( "Demo Player" )]
public class DemoPlayer : PlayerBase
{
	public void GiveWeapon( string className, bool setActive = false )
	{
		var weapon = WeaponRegistry.Instance.Get( className );

		if ( weapon is null )
		{
			Log.Error( $"[SWB Demo] {className} not found in WeaponRegistry!" );
			return;
		}

		Inventory.AddClone( weapon.GameObject, setActive );
		SetAmmo( weapon.Primary.AmmoType, 360 );
	}

    Weapon GetWeapon( string className )
	{
		var weaponGO = Inventory.Items.First( x => x.Name == className );
		if ( weaponGO is not null )
			return weaponGO.Components.Get<Weapon>();

		return null;
	}

	public override void Respawn()
	{
		base.Respawn();
	}

	public override void OnDeath( Shared.DamageInfo info )
	{
		base.OnDeath( info );

		var localPly = PlayerBase.GetLocal();
		if ( localPly is null ) return;

		var display = localPly.RootDisplay as RootDisplay;
		display.AddToKillFeed( info.AttackerId, GameObject.Id, info.Inflictor );

		// Leaderboards
		if ( IsProxy && !IsBot && localPly.GameObject.Id == info.AttackerId )
			Sandbox.Services.Stats.Increment( "kills", 1 );

		if ( !IsProxy && !IsBot )
			Sandbox.Services.Stats.Increment( "deaths", 1 );
	}

	public override void TakeDamage( Shared.DamageInfo info )
	{
		base.TakeDamage( info );

		// Attacker only
		var localPly = PlayerBase.GetLocal();
		if ( localPly is null || !localPly.IsAlive || localPly.GameObject.Id != info.AttackerId ) return;

		var display = localPly.RootDisplay as RootDisplay;
		display.CreateHitmarker( Health <= 0 );
		Sound.Play( "hitmarker" );
	}

	// Arena
    [RequireComponent]
    public UpgradeHolder UpgradeHolder { get; set; }

    protected override void OnAwake()
    {
		base.OnAwake();
		CreateUpgradeScreen();
    }

    public void CreateUpgradeScreen()
	{
		var root = RootDisplay.GameObject;
		root.GetComponent<RootDisplay>().Destroy();
		root.AddComponent<UpgradeMenu>();
	}
}
