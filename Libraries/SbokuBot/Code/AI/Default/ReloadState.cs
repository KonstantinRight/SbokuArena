using Sandbox.Sboku;
using Sandbox.Sboku.Shared;

namespace Sandbox.AI.Default;
internal class ReloadState : StateBase, ICombatState
{
    public ReloadState(SbokuBase bot) : base(bot)
    {
    }
    public override void OnSet()
    {
        Bot.IsReloading = true;
    }

    public override void OnUnset()
    {
    }

    public void OnReloadFinish()
    {
        Bot.SetCombatState<ShootState>();
        Bot.IsReloading = false;
    }
}