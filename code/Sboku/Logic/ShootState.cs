using SWB.Base;
using System;
using System.Diagnostics;

namespace Sandbox.Sboku.Logic;
internal class ShootState : StateBase, ICombatState
{
    public ShootState(SbokuBase bot) : base(bot)
    {
    }

    public override void Think()
    {
            var wep = Adapter.Inventory.Active?.Components.Get<Weapon>();
            if (wep is null)
            {
                Bot.SetCombatState<IdleCombatState>();
                return;
            }

            if (wep.HasAmmo())
            {
                Bot.IsShooting = Scene.Trace.Ray(Bot.EyePos, Target.GameObject.WorldPosition + Bot.HeightToAimAt)
                                            .IgnoreGameObjectHierarchy(Bot.GameObject)
                                            .Run().GameObject?.Parent == Target.GameObject;
            }
            else
            {
                OnReload(wep);
            }
    }

    public override void OnUnset()
    {
        Bot.IsShooting = false;
    }

    public void OnReload(Weapon wep)
    {
        lock (this)
        {
            if (Adapter.Inventory.Active?.Components.Get<Weapon>() != wep)
                return;

            Bot.IsShooting = false;
            Bot.SetCombatState<ReloadState>();
        }
    }
}
