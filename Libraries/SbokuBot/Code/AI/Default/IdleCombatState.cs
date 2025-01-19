using Sandbox.Sboku;
using System;

namespace Sandbox.AI.Default;
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