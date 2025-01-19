using Sandbox.Sboku;

namespace Sandbox.AI.Default;
internal class ShootState : StateBase, ICombatState
{
    public ShootState(SbokuBase bot) : base(bot)
    {
    }

    public override void Think()
    {
        if (Weapon.HasAmmo())
        {
            Bot.IsShooting = Scene.Trace.Ray(Bot.EyePos, Target.GameObject.WorldPosition + Bot.HeightToAimAt)
                                        .IgnoreGameObjectHierarchy(Bot.GameObject)
                                        .Run().GameObject?.Parent == Target.GameObject;
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
