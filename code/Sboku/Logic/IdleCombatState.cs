using System;

namespace Sandbox.Sboku.Logic;
internal class IdleCombatState : StateBase, ICombatState
{
    public IdleCombatState(SbokuBase bot) : base(bot)
    {
    }

    public override void Think()
    {
        if (Target != null && SquaredDistanceToTarget <= MathF.Pow(Bot.MaxFightRange, 2))
        {
            Bot.SetCombatState<ShootState>();
        }
    }

    public override void OnSet()
    {
        Bot.IsShooting = false;
    }
}