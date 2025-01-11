using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Citizen;
using Sandbox.Events;
using Sandbox.Sboku.Logic;
using Sandbox.Sboku.States;
using SWB.Base;
using SWB.Shared;

namespace Sandbox.Sboku;
public class SbokuBase : Component, IGameEventHandler<Weapon.NoAmmoLeftEvent>, IGameEventHandler<Weapon.ReloadFinished>
{
    [Group("Controller")]
    [Property]
    public float Velocity { get; set; } = 160f;
    [Group("Controller")]
    [Property]
    public float Friction { get; set; } = 4.0f;
    [Group("Controller")]
    [Property]
    public float MaxForce { get; set; } = 50f;
    [Group("Controller")]
    [Property]
    public float AirControl { get; set; } = 0.1f;

    [Group("AI")]
    [Property]
    [Range(100, 5000, step: 100)]
    public int SearchRange { get; set; } = 1500;
    [Group("AI")]
    [Property]
    [Range(100, 5000, step: 100)]
    public int MinFightRange { get; set; } = 400;
    [Group("AI")]
    [Property]
    [Range(100, 5000, step: 100)]
    public int MaxFightRange { get; set; } = 600;
    [Group("AI")]
    [Property]
    public bool IsOffline { get; set; } = false;

    public int DistanceToRecalucaltePath { get => MinFightRange / 2; }
    public float ThinkingInterval { get => Settings.ThinkingInterval; }
    public Angles EyeAngles { get => GetComponent<SWBAdapter>().EyeAngles; set => GetComponent<SWBAdapter>().EyeAngles = value; }
    public Vector3 EyePos { get => GetComponent<SWBAdapter>().EyePos; }

    /// <summary>
    /// A point in space the bot is navigating toward
    /// </summary>
    public Vector3? Destination { get; private set; }

    /// <summary>
    /// Target the bot must attack
    /// </summary>
    public IPlayerBase Target { get; set; } = null;

    public bool IsShooting { get; set; }

    public bool IsNavigating { get => path != null; }
    /// <summary>
    /// Height bots will aim at
    /// </summary>
    public Vector3 HeightToAimAt { get => Target != null ? Vector3.Zero.WithZ(Target.GameObject.GetComponent<CharacterController>().Height * 2 / 3) : Vector3.Zero; }

    /// <summary>
    /// You must set it to true to reload and then manually unset. This way is more robust due to the way SWB works.
    /// </summary>
    public bool IsReloading { get; set; }

    private CharacterController character => GetComponentInChildren<CharacterController>();
    private CitizenAnimationHelper anim => GetComponentInChildren<CitizenAnimationHelper>();
    public SbokuSettings Settings { get; private set; }

    private List<Vector3> path;
    private int pathEnumerator;

    #region States
    
    private Dictionary<Type, ISbokuState> states;
    private IActionState actionState;
    private ICombatState combatState;
    private List<ICondition> conditions;

    public void SetActionState<T>() where T : IActionState
    {
        var state = (IActionState)states[typeof(T)];
        actionState?.OnUnset();
        state.OnSet();
        actionState = state;
    }
    public void SetCombatState<T>() where T : ICombatState
    {
        var state = (ICombatState)states[typeof(T)];
        combatState?.OnUnset();
        state.OnSet();
        combatState = state;
    }
    public bool IsActiveActionState<T>() where T : IActionState
        => actionState.GetType() == typeof(T);
    public bool IsActiveCombatState<T>() where T : ICombatState
        => combatState.GetType() == typeof(T);
    #endregion

    public SbokuBase()
    {
        states = new()
        {
            { typeof(IdleActionState), new IdleActionState(this) },
            { typeof(ChaseState), new ChaseState(this) },
            { typeof(TacticalState), new TacticalState(this) },
            { typeof(ShootState), new ShootState(this) },
            { typeof(IdleCombatState), new IdleCombatState(this) },
            { typeof(ReloadState), new ReloadState(this) },
        };

        SetActionState<IdleActionState>();
        SetCombatState<IdleCombatState>();
        conditions = Conditions.Get(this);
    }

    #region Component events

    private TimerHelper timer = new();
    private object TimerHandler;
    protected override void OnEnabled()
    {
        TimerHandler = timer.Every(ThinkingInterval, OnStateExecute);

        if (MinFightRange > MaxFightRange)
        {
            Log.Error("Min fight range is supposed to be less than MaxFightRange");
        }
    }
    protected override void OnDisabled()
    {
        if (TimerHandler is not null)
            timer.Remove(TimerHandler);
    }
    protected override void OnStart()
    {
        ClothingContainer.CreateFromLocalUser().Apply(GetComponentInChildren<SkinnedModelRenderer>());
    }
    protected override void OnAwake()
    {
        Settings = SbokuSettings.CreateOrFind(Scene);
        if (!Scene.NavMesh.IsEnabled)
        {
            Enabled = false;
            Log.Error("NavMesh must be enabled");
        }
    }
    protected void OnStateExecute()
    {
        if (IsOffline || IsProxy || Scene.NavMesh.IsGenerating) return;

        foreach (var cond in conditions)
        {
            if (cond.If())
            {
                cond.Then();
                if (cond.IsTerminal())
                    break;
            }
        }

        actionState.Think();
        combatState.Think();
    }
    protected override void OnUpdate()
    {
        timer.OnUpdate();

        var swb = GetComponent<SWBAdapter>();
        if (!swb.IsAlive)
        {
            Enabled = false;
        }
    }
    protected override void OnFixedUpdate()
    {
        if (path is not null)
        {
            if (Settings.ShowDebugOverlay)
            {
                foreach (var p in path)
                    Scene.DebugOverlay.Sphere(new Sphere(p, 10), Color.Yellow, 1);
            }

            if (Vector3.DistanceBetweenSquared(character.WorldPosition.WithZ(0), Destination.Value.WithZ(0)) < MathF.Pow(Scene.NavMesh.AgentRadius, 2))
            {
                pathEnumerator++;
                if (pathEnumerator < path.Count)
                {
                    Destination = path[pathEnumerator];
                }
                else
                {
                    path = null;
                    Destination = null;
                    pathEnumerator = 0;
                }
            }
        }
        var vector = Vector3.Zero;
        if (Destination is Vector3 dest)
        {
            var direction = dest - character.WorldPosition;
            float yaw = MathF.Atan2(direction.y, direction.x).RadianToDegree();
            vector = direction.WithZ(0).Normal;
            Rotate(yaw);
        }

        if (Target is IPlayerBase ply)
        {
            var direction = ply.GameObject.WorldPosition - character.WorldPosition;
            float yaw = MathF.Atan2(direction.y, direction.x).RadianToDegree();
            Rotate(yaw);

            direction = ply.GameObject.WorldPosition + HeightToAimAt - EyePos;
            float pitch = -MathF.Atan2(direction.z, /* Length2D */ MathF.Sqrt(direction.x * direction.x + direction.y * direction.y)).RadianToDegree();
            EyeAngles = new Angles(pitch, GameObject.WorldRotation.Yaw(), 0);
        }

        Move(vector);

        UpdateAnimations(vector, character.WorldRotation);
    }

    protected override void DrawGizmos()
    {
        base.DrawGizmos();

        Gizmo.Draw.Color = Color.Green;
        Gizmo.Draw.LineCircle(Vector3.Zero, Vector3.Up, SearchRange);
        Gizmo.Draw.Color = Color.Orange;
        Gizmo.Draw.LineCircle(Vector3.Zero, Vector3.Up, MinFightRange);
        Gizmo.Draw.Color = Color.Red;
        Gizmo.Draw.LineCircle(Vector3.Zero, Vector3.Up, MaxFightRange);
    }
   
    #endregion

    /// <summary>
    /// Try to move in the direction given by the wishVelocity unit vector
    /// </summary>
    /// <param name="wishVelocity"></param>
    private void Move(Vector3 wishVelocity)
    {
        var vel = wishVelocity * Velocity;
        var gravity = Scene.PhysicsWorld.Gravity;
        if (character.IsOnGround)
        {
            character.Velocity = character.Velocity.WithZ(0);
            character.Accelerate(vel);
            character.ApplyFriction(Friction);
        }
        else
        {
            character.Velocity += gravity * Time.Delta * 0.5f;
            character.Accelerate(vel.ClampLength(MaxForce));
            character.ApplyFriction(AirControl);
        }

        if (!(character.Velocity.IsNearZeroLength && vel.IsNearZeroLength))
        {
            character.Move();
        }
    }

    /// <summary>
    /// Set rotation based on the yaw
    /// </summary>
    /// <param name="yaw"></param>
    private void Rotate(float yaw)
        => GameObject.WorldRotation = Rotation.FromYaw(yaw);

    private void UpdateAnimations(Vector3 WishVelocity, Rotation rotation)
    {
        if (anim is null) return;

        anim.WithWishVelocity(WishVelocity);
        anim.WithVelocity(character.Velocity);
        anim.AimAngle = rotation;
        anim.IsGrounded = character.IsOnGround;
        anim.WithLook(rotation.Forward);
        anim.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
    }

    /// <summary>
    /// Navigate to the position
    /// </summary>
    /// <param name="targetPosition"></param>
    public void MoveTo(Vector3 targetPosition)
        => MoveTo(Scene.NavMesh.GetSimplePath(GameObject.WorldPosition, targetPosition));

    /// <summary>
    /// Navigate to the position given the path.
    /// </summary>
    /// <param name="path"></param>
    public void MoveTo(List<Vector3> path)
    {
        if (!path.Any())
        {
            Log.Info("Path contains no elements");
            return;
        }

        this.path = path;
        pathEnumerator = 0;
        Destination = path[pathEnumerator];
    }
    /// <summary>
    /// Stop moving to the point, given by the MoveTo methods
    /// </summary>
    public void StopNavigating()
    {
        path = null;
        Destination = null;
    }


    public void OnGameEvent(Weapon.NoAmmoLeftEvent eventArgs)
    {
        (combatState as ShootState)?.OnReload(eventArgs.Weapon);
    }

    public void OnGameEvent(Weapon.ReloadFinished eventArgs)
    {
        (combatState as ReloadState)?.OnReloadFinish(eventArgs.Weapon);
    }
}
