using Sandbox.Sboku;

namespace Sandbox.AI.Default;
internal class ShootState : StateBase, ICombatState
{
    private Burst burst;
    public ShootState(SbokuBase bot) : base(bot)
    {
    }

    private record Burst
    {
        public float Period;
        public TimeSince Timer = new TimeSince();
        public Burst(float period)
        {
            Period = period;
            Timer = 0;
        }
        public bool CanFire()
            => Timer < Period;
        public bool ShouldStop()
            => Timer > Period;
        public bool CanContinue()
            => Timer > Period * 2;
    }

    public override void Think()
    {
        if (Weapon.HasAmmo())
        {
            if (burst?.CanFire() ?? true)
            {
                Bot.IsShooting = Scene.Trace.Ray(Bot.EyePos, Target.GameObject.WorldPosition + Bot.HeightToAimAt)
                                            .IgnoreGameObjectHierarchy(Bot.GameObject)
                                            .Run().GameObject?.Parent == Target.GameObject;
            }
            else if (burst != null)
            {
                if (burst.CanContinue())
                    burst = null;
                else if (burst.ShouldStop())
                    Bot.IsShooting = false;
            }
            
            if (Bot.IsShooting && burst == null)
            {
                burst = new(Bot.BurstPeriod);
            }
        }
        else
        {
            OnReload();
        }
    }

    public override void OnUnset()
    {
        Bot.IsShooting = false;
    }

    public void OnReload()
    {
        lock (this)
        {
            Bot.IsShooting = false;
            Bot.SetCombatState<ReloadState>();
        }
    }
}
