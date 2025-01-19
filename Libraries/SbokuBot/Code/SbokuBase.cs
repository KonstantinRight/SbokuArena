using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.AI.Default;
using Sandbox.Sboku.Shared;
using Sandbox.Shared;

namespace Sandbox.Sboku;
public abstract class SbokuBase : Component, ISbokuBot
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
    [Group("Controller")]
    [Property]
    public CharacterController Character { get; set; }

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
    /// <summary>
    /// If true, the bot won't make any new decisions
    /// </summary>
    [Group("AI")]
    [Property]
    public bool IsOffline { get; set; } = false;

    public int DistanceToRecalucaltePath { get => MinFightRange / 2; }
    public float ThinkingInterval { get => Settings.ThinkingInterval; }
    public abstract Angles EyeAngles { get; set; }
    public abstract Vector3 EyePos { get; }

    /// <summary>
    /// A point in space the bot is navigating toward
    /// </summary>
    public Vector3? Destination { get; private set; }

    /// <summary>
    /// Target the bot must attack
    /// </summary>
    public ISbokuTarget Target { get; set; } = null;

    /// <summary>
    /// Active weapon of the bot
    /// </summary>
    public abstract ISbokuWeapon Weapon { get; }

    public bool IsShooting { get; set; }

    public bool IsNavigating { get => path != null; }
    /// <summary>
    /// Height bots will aim at
    /// </summary>
    public Vector3 HeightToAimAt { get => Target != null ? Vector3.Zero.WithZ(Target.CharacterController.Height * 2 / 3) : Vector3.Zero; }

    /// <summary>
    /// You must set it to true to reload and then manually unset. This way is more robust due to the way SWB works.
    /// </summary>
    public bool IsReloading { get; set; }

    public SbokuSettings Settings { get; private set; }

    private List<Vector3> path;
    private int pathEnumerator;

    #region States

    protected Dictionary<Type, ISbokuState> States { get; private set; }
    private IActionState actionState;
    private ICombatState combatState;
    private List<ISbokuCondition> conditions;

    public void SetActionState<T>() where T : IActionState
    {
        var state = (IActionState)States[typeof(T)];
        actionState?.OnUnset();
        state.OnSet();
        actionState = state;
    }
    public void SetCombatState<T>() where T : ICombatState
    {
        var state = (ICombatState)States[typeof(T)];
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
        States = GetStates();
        conditions = GetConditions();

        SetActionState<IdleActionState>();
        SetCombatState<IdleCombatState>();
    }

    protected virtual Dictionary<Type, ISbokuState> GetStates()
        => new()
        {
            { typeof(IdleActionState), new IdleActionState(this) },
            { typeof(ChaseState), new ChaseState(this) },
            { typeof(TacticalState), new TacticalState(this) },
            { typeof(ShootState), new ShootState(this) },
            { typeof(IdleCombatState), new IdleCombatState(this) },
            { typeof(ReloadState), new ReloadState(this) },
        };

    protected virtual List<ISbokuCondition> GetConditions()
        => Conditions.Get(this);

    public void ResetState()
    {
        Target = null;
        Destination = null;
        SetActionState<IdleActionState>();
        SetCombatState<IdleCombatState>();
    }

    #region Component events

    private TimerHelper timer = new();
    private object TimerHandler;
    protected override void OnEnabled()
    {
        if (MinFightRange > MaxFightRange)
        {
            Log.Error("Min fight range is supposed to be less than MaxFightRange");
        }

        if (IsProxy)
            return;

        TimerHandler = timer.Every(ThinkingInterval, OnStateExecute);
    }
    protected override void OnDisabled()
    {
        ResetState();

        if (TimerHandler is not null)
            timer.Remove(TimerHandler);
    }
    protected override void OnAwake()
    {
        Settings = SbokuSettings.CreateOrFind(Scene);

        if (Character == null)
        {
            Character = AddComponent<CharacterController>();
        }

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
        if (IsProxy)
            return;

        timer.OnUpdate();
    }
    protected override void OnFixedUpdate()
    {
        if (IsProxy)
            return;

        if (path is not null)
        {
            if (Settings.ShowDebugOverlay)
            {
                foreach (var p in path)
                    Scene.DebugOverlay.Sphere(new Sphere(p, 10), Color.Yellow, 1);
            }

            if (Vector3.DistanceBetweenSquared(Character.WorldPosition.WithZ(0), Destination.Value.WithZ(0)) < MathF.Pow(Scene.NavMesh.AgentRadius, 2))
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
            var direction = dest - Character.WorldPosition;
            float yaw = MathF.Atan2(direction.y, direction.x).RadianToDegree();
            vector = direction.WithZ(0).Normal;
            Rotate(yaw);
        }

        if (Target is ISbokuTarget ply)
        {
            var direction = ply.GameObject.WorldPosition - Character.WorldPosition;
            float yaw = MathF.Atan2(direction.y, direction.x).RadianToDegree();
            Rotate(yaw);

            direction = ply.GameObject.WorldPosition + HeightToAimAt - EyePos;
            float pitch = -MathF.Atan2(direction.z, /* Length2D */ MathF.Sqrt(direction.x * direction.x + direction.y * direction.y)).RadianToDegree();
            EyeAngles = new Angles(pitch, GameObject.WorldRotation.Yaw(), 0);
        }

        Move(vector);

        UpdateAnimations(vector, Character.WorldRotation);
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
    protected virtual void Move(Vector3 wishVelocity)
    {
        var vel = wishVelocity * Velocity;
        var gravity = Scene.PhysicsWorld.Gravity;
        if (Character.IsOnGround)
        {
            Character.Velocity = Character.Velocity.WithZ(0);
            Character.Accelerate(vel);
            Character.ApplyFriction(Friction);
        }
        else
        {
            Character.Velocity += gravity * Time.Delta * 0.5f;
            Character.Accelerate(vel.ClampLength(MaxForce));
            Character.ApplyFriction(AirControl);
        }

        if (!(Character.Velocity.IsNearZeroLength && vel.IsNearZeroLength))
        {
            Character.Move();
        }
    }

    /// <summary>
    /// Set rotation based on the yaw
    /// </summary>
    /// <param name="yaw"></param>
    private void Rotate(float yaw)
        => GameObject.WorldRotation = Rotation.FromYaw(yaw);

    [Rpc.Broadcast]
    protected abstract void UpdateAnimations(Vector3 WishVelocity, Rotation rotation);

    /// <summary>
    /// Navigate to the position
    /// </summary>
    /// <param name="targetPosition"></param>
    public void MoveTo(Vector3 targetPosition)
        => MoveTo(Scene.NavMesh.GetSimplePathSafe(GameObject.WorldPosition, targetPosition));

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

    public void Reload()
    {
        (combatState as ShootState)?.OnReload();
    }

    public void OnReloadFinish()
    {
        (combatState as ReloadState)?.OnReloadFinish();
    }
}
