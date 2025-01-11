using System;
using Sandbox.Sboku.States;
using System.Collections.Generic;

namespace Sandbox.Sboku.Logic;
public class Conditions
{
    private abstract class SimpleCondition : SbokuParent, ICondition
    {
        public SimpleCondition(SbokuBase bot) : base(bot)
        {
        }

        public abstract bool If();
        public abstract void Then();

        public bool IsTerminal()
            => false;

    }
    private class StopCondion : SimpleCondition
    {
        public StopCondion(SbokuBase bot) : base(bot)
        {
        }
        public override bool If()
                =>    !(Bot.IsActiveActionState<IdleActionState>() && Bot.IsActiveCombatState<IdleCombatState>())
                   && (Target == null 
                   || !Target.IsValid
                   || !Target.IsAlive
                   || SquaredDistanceToTarget > MathF.Pow(Bot.SearchRange, 2));
        public override void Then()
        {
            Bot.Target = null;
            Bot.SetActionState<IdleActionState>();
            Bot.SetCombatState<IdleCombatState>();
        }
    }
    private class ChaseCondition : SimpleCondition
    {
        public ChaseCondition(SbokuBase bot) : base(bot)
        {
        }
        public override bool If()
                => Bot.Target != null && SquaredDistanceToTarget > MathF.Pow(Bot.MaxFightRange, 2);
        public override void Then()
            => Bot.SetActionState<ChaseState>();
    }


    public static List<ICondition> Get(SbokuBase bot) =>
        new List<ICondition>()
        {
            new StopCondion(bot),
            new ChaseCondition(bot)
        };
}
