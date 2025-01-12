using Sandbox.Citizen;
using Sandbox.Sboku.Arena;
using SWB.Base;
using SWB.Player;
using SWB.Shared;
using System;

namespace Sandbox.Sboku;
[Title("SWB Adapter")]
public class SWBAdapter : Component, IPlayerBase
{
    public void GiveWeapon(string className, bool setActive = false)
    {
        var weapon = WeaponRegistry.Instance.Get(className);

        if (weapon is null)
        {
            Log.Error($"[SWB Demo] {className} not found in WeaponRegistry!");
            return;
        }

        Inventory.AddClone(weapon.GameObject, setActive);
        SetAmmo(weapon.Primary.AmmoType, 360);
    }

    protected override void OnStart()
    {
        if (IsProxy) return;

        Health = MaxHealth;
        Inventory = Components.Create<Inventory>();
        InitCameras();
        GiveWeapon("swb_scarh", true);
    }

    protected override void OnAwake()
    {
        sboku = GetComponent<SbokuBase>();
    }

    #region Sboku

    private SbokuBase sboku;

    public bool IsAttackPressed(string type)
        => sboku?.IsShooting ?? false;
    public bool IsAttackDown(string type)
        => sboku?.IsShooting ?? false;
    public bool IsReloadDown()
        => sboku?.IsReloading ?? false;

    #endregion

    #region Properties

    [Property]
    public GameObject Head { get; set; }
    [Property]
    public GameObject Body { get; set; }
    [Property]
    public SkinnedModelRenderer BodyRenderer { get; set; }
    [Property]
    public CitizenAnimationHelper AnimationHelper { get; set; }
    [Property]
    public int MaxHealth { get; set; } = 100;

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
    Vector3 IPlayerBase.Velocity => GetComponent<SbokuBase>().Velocity;
    public bool IsOnGround => GetComponent<CharacterController>().IsOnGround;
    public bool IsAlive => Health > 0;
    public Vector3 EyePos => Head.WorldPosition + EyeOffset;
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
    public Angles EyeAngles { get; set; }

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

    [Property]
    public ModelPhysics RagdollPhysics { get; set; }

    [Rpc.Broadcast]
    public void TakeDamage(SWB.Shared.DamageInfo info)
    {
        if (!IsValid || IsProxy || !IsAlive)
            return;

        if (Array.Exists(info.Tags, tag => tag == "head"))
            info.Damage *= 2;

        var multiplier = 1f;
        var holder = GetComponent<UpgradeHolder>();
        if (holder != null)
        {
            multiplier = holder.ArmorMultiplier;
        }
        Health -= (int)(MathF.Round(info.Damage * multiplier));

        if (Health <= 0)
            OnDeath(info);
    }

    [Rpc.Broadcast]
    public virtual void OnDeath(SWB.Shared.DamageInfo info)
    {
        if (!IsValid) return;
        var attackerGO = Scene.Directory.FindByGuid(info.AttackerId);

        if (attackerGO is not null && !attackerGO.IsProxy)
        {
            var attacker = attackerGO.Components.Get<IPlayerBase>();

            if (attacker is not null && attacker != this)
                attacker.Kills++;
        }

        if (IsProxy) return;

        Deaths++;
        GetComponent<CharacterController>().Velocity = 0;
        Ragdoll(info.Force, info.Origin);
        Inventory.Clear();
    }
    [Rpc.Broadcast]
    public virtual void Ragdoll(Vector3 force, Vector3 forceOrigin)
    {
        if (!IsValid) return;

        Tags.Add(TagsHelper.DeadPlayer);
        ToggleColliders(false);
        RagdollPhysics.Enabled = true;

        foreach (var body in RagdollPhysics.PhysicsGroup.Bodies)
        {
            body.ApplyImpulseAt(forceOrigin, force);
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
