using SWB.Base;

namespace Sandbox.Sboku.Logic;
internal class ReloadState : StateBase, ICombatState
{
    public ReloadState(SbokuBase bot) : base(bot)
    {
    }
    public override void OnSet()
    {
        if (SwitchToIdleIfNoWeapon()) return;

        Bot.IsReloading = true;
    }

    public override void OnUnset()
    {
    }

    public void OnReloadFinish(Weapon wep)
    {
        var active = Adapter.Inventory.Active?.GetComponent<Weapon>();
        if (active is not null && active == wep)
        {
            Bot.SetCombatState<ShootState>();
            Bot.IsReloading = false;
        }
    }

    private bool SwitchToIdleIfNoWeapon()
    {
        var active = Adapter.Inventory.Active;
        if (active is null)
        {
            Bot.SetCombatState<IdleCombatState>();
            return true;
        }
        return false;
    }
}