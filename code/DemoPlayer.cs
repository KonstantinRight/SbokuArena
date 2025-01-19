using Sandbox.Events;
using Sandbox.Sboku.Arena;
using Sandbox.Sboku.UI;
using SWB.Base;
using SWB.HUD;
using SWB.Player;
using System.Linq;

namespace SWB.Demo;

[Group( "SWB" )]
[Title( "Demo Player" )]
public class DemoPlayer : PlayerBase, IGameEventHandler<RoundManager.OpenUpgradeScreen>, IGameEventHandler<RoundManager.Victory>
{
    [Property]
    public GameObject ArenaUI { get; set; }

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

        if (IsBot) return;

        // Give weapons
        //GiveWeapon("swb_colt");
        //GiveWeapon("swb_revolver");
        //GiveWeapon("swb_remington");
        //GiveWeapon("swb_veresk");
        //GiveWeapon("swb_scarh", true);
        //GiveWeapon("swb_l96a1");
    }

	public override void OnDeath( Shared.DamageInfo info )
	{
		base.OnDeath( info );

		//var localPly = PlayerBase.GetLocal();
		//if ( localPly is null ) return;

		//var display = localPly.RootDisplay as RootDisplay;
		//display.AddToKillFeed( info.AttackerId, GameObject.Id, info.Inflictor );

		//// Leaderboards
		//if ( IsProxy && !IsBot && localPly.GameObject.Id == info.AttackerId )
		//	Sandbox.Services.Stats.Increment( "kills", 1 );

		//if ( !IsProxy && !IsBot )
		//	Sandbox.Services.Stats.Increment( "deaths", 1 );
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

    public void CreateHitmarker(int health)
    {
        if (IsProxy)
            return;

        var display = RootDisplay as RootDisplay;
        display.CreateHitmarker(health <= 0);
        Sound.Play("hitmarker");
    }

    // Arena

    protected override void OnStart()
    {
        base.OnStart();

        if (IsProxy || !IsValid)
        {
            ArenaUI.Enabled = false;
            return;
        }

        CreateUpgradeScreen();
    }

    private void HideUI()
	{
        if (RootDisplay.GetComponent<ScreenPanel>() != null)
            RootDisplay.GetComponent<ScreenPanel>().Enabled = false;
    }

    public void CreateUpgradeScreen()
	{
		HideUI();
		ArenaUI.AddComponent<UpgradeMenu>();
	}

    public void CreateFailScreen()
    {
        HideUI();
        ArenaUI.AddComponent<EndMenu>();
    }

    public void CreateVictoryScreen()
	{
        HideUI();
        var c = ArenaUI.AddComponent<EndMenu>();
        c.WinVariant = true;
    }

    public void OnGameEvent(RoundManager.OpenUpgradeScreen eventArgs)
    {
		if (!IsValid || IsProxy) return;
        CreateUpgradeScreen();
    }

    public void OnGameEvent(RoundManager.Victory eventArgs)
    {
        if (!IsValid || IsProxy) return;
		CreateVictoryScreen();
    }
}
