using Sandbox.Citizen;
using Sandbox.Events;
using Sandbox.Sboku.Arena;
using Sandbox.Sboku.Shared;
using SWB.Base;
using SWB.Shared;
using SWB.Demo;
using SWB.Player;
using System;
using System.Linq;

namespace Sandbox.Sboku;

[Title("Bot Adapter")]
[Group("Sboku Arena")]
public class BotAdapter : SbokuBase, IPlayerBase, IGameEventHandler<Weapon.NoAmmoLeftEvent>, IGameEventHandler<Weapon.ReloadFinished>
{
	public GameObject RagdollGO { get; set; }

	[RequireComponent]
    public UpgradeHolder UpgradeHolder { get; set; }

    public void GiveWeapon(string className)
    {
        var weapon = WeaponRegistry.Instance.Get(className);

        if (weapon is null)
        {
            Log.Error($"[SWB Demo] {className} not found in WeaponRegistry!");
            return;
        }

        Inventory.AddClone(weapon.GameObject, true);
        SetAmmo(weapon.Primary.AmmoType, 360);
        sbokuWeapon = new WeaponAdapter(weapon);
    }

    protected override void OnStart()
    {
        ClothingContainer.CreateFromLocalUser().Apply(GetComponentInChildren<SkinnedModelRenderer>());

        if (IsProxy) return;
        Health = MaxHealth;
        Inventory = Components.Create<Inventory>();
        InitCameras();
        var wep = WeaponRegistry.Instance.Weapons.Values.ElementAt(Game.Random.Next(0, WeaponRegistry.Instance.Weapons.Count));
        //var wep = WeaponRegistry.Instance.Get("swb_scarh");
        GiveWeapon(wep.ClassName);
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        sboku = GetComponent<SbokuBase>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!IsAlive)
        {
            Enabled = false;
        }
    }

	#region Inherited

	public bool IsClimbingLadder => false;

	public bool IsBot { get => false; set { } }
	public float FieldOfView { get => Camera.FieldOfView; set => Camera.FieldOfView = value; }
	public HoldTypes HoldType
	{
		set { AnimationHelper.HoldType = (CitizenAnimationHelper.HoldTypes)value; }
	}

	public void TriggerAnimation( Animations animation )
	{
		string animationName = animation switch
		{
			Animations.Attack => "b_attack",
			Animations.Reload => "b_reload",
			_ => ""
		};

		if ( animationName == "" ) return;
		BodyRenderer.Set( animationName, true );
	}

	public void ApplyEyeAnglesOffset( Angles offset )
	{
		
	}

	public void ParentToBone( GameObject weaponObject, string boneName )
	{
		ModelUtil.ParentToBone( weaponObject, BodyRenderer, boneName );
	}

	public void OnDamage( in DamageInfo info )
	{
		if ( info is not SWB.Shared.DamageInfo )
		{
			Log.Warning( "PlayerBase: OnDamage called with non-Shared.DamageInfo. Ignoring." );
			return;
		}
		info.Shape = null; // Remove physics shape to avoid issues with networking
		info.Hitbox = null; // Remove hitbox to avoid issues with networking
		TakeDamage( info as SWB.Shared.DamageInfo );
	}

	#endregion

	#region Sboku

	private CitizenAnimationHelper anim => GetComponentInChildren<CitizenAnimationHelper>();
    
    protected override void UpdateAnimations(Vector3 WishVelocity, Rotation rotation)
    {
        if (anim is null) return;

        anim.WithWishVelocity(WishVelocity);
        anim.WithVelocity(Character.Velocity);
        anim.AimAngle = rotation;
        anim.IsGrounded = Character.IsOnGround;
        anim.WithLook(rotation.Forward);
        anim.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
    }

    public override ISbokuWeapon Weapon { get => sbokuWeapon; }
    private ISbokuWeapon sbokuWeapon;

    private SbokuBase sboku;

    protected override void Move(Vector3 wishVelocity)
    {
        base.Move(wishVelocity * new Vector3(UpgradeHolder.SpeedMultiplier, UpgradeHolder.SpeedMultiplier, 1));
    }

    public bool IsAttackPressed(string type)
        => sboku?.IsShooting ?? false;
    public bool IsAttackDown(string type)
        => sboku?.IsShooting ?? false;
    public bool IsReloadDown()
        => sboku?.IsReloading ?? false;

    public void OnGameEvent(Weapon.NoAmmoLeftEvent eventArgs)
        => Reload();

    public void OnGameEvent(Weapon.ReloadFinished eventArgs)
        => OnReloadFinish();

    #endregion

    #region Properties

    [Group("Character")]
    [Property]
    public GameObject Head { get; set; }
    [Group("Character")]
    [Property]
    public GameObject Body { get; set; }
    [Group("Character")]
    [Property]
    public SkinnedModelRenderer BodyRenderer { get; set; }
    [Group("Character")]
    [Property]
    public CitizenAnimationHelper AnimationHelper { get; set; }
    [Group("Character")]
    [Property]
    public int MaxHealth { get; set; } = 100;
    [Group("Character")]
    [Property]
    public ModelPhysics RagdollPhysics { get; set; }

    #endregion

    #region Camera

    public CameraComponent Camera { get; set; }
    public CameraComponent ViewModelCamera { get; set; }

    private void InitCameras()
    {
        Camera = AddComponent<CameraComponent>(false);
        ViewModelCamera = AddComponent<CameraComponent>(false);
    }

    #endregion

    #region Expression bodied

    public bool IsFirstPerson => false;
    Vector3 IPlayerBase.Velocity => GetComponent<SbokuBase>()?.Velocity ?? Vector3.Zero;
    public bool IsOnGround => GetComponent<CharacterController>()?.IsOnGround ?? true;
    public bool IsAlive => Health > 0;
    public override Vector3 EyePos => Head.WorldPosition + EyeOffset;
    Guid IPlayerBase.Id { get => GameObject.Id; }

    #endregion

    #region etc

    public IInventory Inventory { get; set; }
    public float InputSensitivity { get; set; }
    public Angles EyeAnglesOffset { get; set; }

    [Sync] 
    public Vector3 EyeOffset { get; set; } = Vector3.Zero;
    [Sync] 
    public bool IsCrouching { get; set; }
    [Sync] 
    public bool IsRunning { get; set; }
    [Sync]
    public int Health { get; set; }
    [Sync]
    public int Kills { get; set; }
    [Sync]
    public int Deaths { get; set; }
    [Sync]
    public override Angles EyeAngles { get => eyeAngles; set => eyeAngles = value; }
    private Angles eyeAngles;

    public void ShakeScreen(ScreenShake screenShake)
    {

    }

    #endregion

    #region Ammo

    [Sync]
    public NetDictionary<string, int> Ammo { get; set; } = new();

	public virtual int AmmoCount(string ammoType)
    {
        if (Ammo.TryGetValue(ammoType, out var amount))
        {
            return amount;
        }

        return 0;
    }

    public virtual void SetAmmo(string ammoType, int amount)
    {
        Ammo[ammoType] = amount;
    }

    public virtual int TakeAmmo(string ammoType, int amount)
    {
        var available = AmmoCount(ammoType);
        amount = Math.Min(available, amount);

        SetAmmo(ammoType, available - amount);

        return amount;
    }

    #endregion

    #region Damage

    [Rpc.Broadcast]
    public void TakeDamage(SWB.Shared.DamageInfo info)
    {
        var attacker = Scene.Directory.FindByGuid(info.Attacker.Id);
        if (attacker == null || !attacker.IsValid) 
            return;

        if (IsValid && !IsProxy && IsAlive)
        {
            if (info.Tags.Contains("string"))
                info.Damage *= 2;

            float dmgMultiplier = 1;
            if (attacker != null && attacker.IsValid)
            {
                dmgMultiplier = attacker.GetComponent<UpgradeHolder>().DamageMultiplier;
            }

            Health -= (int)(MathF.Round(info.Damage * GetComponent<UpgradeHolder>().ArmorMultiplier * dmgMultiplier));

            if (Health <= 0)
                OnDeath(info);
        }

        var ply = attacker.GetComponent<DemoPlayer>();
        if (ply is not null && ply.IsValid())
        {
            ply.CreateHitmarker(Health);
        }
    }

    [Rpc.Broadcast]
    public virtual void OnDeath(SWB.Shared.DamageInfo info)
    {
        if (!IsValid) return;
        var attackerGO = Scene.Directory.FindByGuid(info.Attacker.Id);

        if (attackerGO is not null && !attackerGO.IsProxy)
        {
            var attacker = attackerGO.Components.Get<IPlayerBase>();

            //if (attacker is not null && attacker != this)
            //    attacker.Kills++;
        }

        if (IsProxy) return;

        Deaths++;
		var controller = GetComponent<CharacterController>();
		controller.Velocity = 0;
        Ragdoll(info.Force, info.Origin, controller.Velocity );
        Inventory.Clear();
    }
    [Rpc.Broadcast]
    public virtual void Ragdoll(Vector3 force, Vector3 forceOrigin, Vector3 velocity )
    {
		if ( !IsValid ) return;
		CreateRagdoll( force, forceOrigin, velocity );
		Body.Enabled = false;
	}

	public virtual void CreateRagdoll( Vector3 force, Vector3 forceOrigin, Vector3 velocity )
	{
		RagdollGO = new GameObject( true, "Ragdoll" );
		RagdollGO.Tags.Add( TagsHelper.DeadPlayer );
		RagdollGO.NetworkMode = NetworkMode.Never;
		RagdollGO.WorldPosition = WorldPosition;
		RagdollGO.WorldRotation = Body.WorldRotation;

		// Renderer
		var renderer = RagdollGO.AddComponent<SkinnedModelRenderer>();
		renderer.Model = BodyRenderer.Model;
		renderer.UseAnimGraph = false;
		renderer.Sequence.Name = "Eyes_Closed";

		// Clothes
		ClothingContainer.CreateFromLocalUser().Apply( RagdollGO.GetComponentInChildren<SkinnedModelRenderer>() );

		// Physics
		var physics = RagdollGO.AddComponent<ModelPhysics>( true );
		physics.Model = renderer.Model;
		physics.Renderer = renderer;
		physics.CopyBonesFrom( BodyRenderer, true );

		var forceMultiplier = IsOnGround ? 200 : 100;
		velocity *= forceMultiplier;

		foreach ( var body in physics.Bodies )
		{
			body.Component.ApplyForceAt( renderer.SceneModel.Bounds.Center, velocity );
			body.Component.ApplyImpulseAt( forceOrigin, force );
		}
	}

	public virtual void ToggleColliders(bool enable)
    {
        var colliders = Body.Components.GetAll<Collider>(FindMode.EverythingInSelfAndParent);

        foreach (var collider in colliders)
        {
            collider.Enabled = enable;
        }
    }

	#endregion
}
