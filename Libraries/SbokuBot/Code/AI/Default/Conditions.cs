using System;
using System.Collections.Generic;
using Sandbox.Sboku;
using Sandbox.Shared;

namespace Sandbox.AI.Default;
public class Conditions
{
    private abstract class SimpleCondition : SbokuParent, ISbokuCondition
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
                => !(Bot.IsActiveActionState<IdleActionState>() && Bot.IsActiveCombatState<IdleCombatState>())
                   && (Weapon == null
                   ||  Target == null
                   || !Target.IsValid
                   || !Target.IsAlive
                   || SquaredDistanceToTarget > MathF.Pow(Bot.SearchRange, 2));
        public override void Then()
            => Bot.ResetState();
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


    public static List<ISbokuCondition> Get(SbokuBase bot) =>
        new List<ISbokuCondition>()
        {
            new StopCondion(bot),
            new ChaseCondition(bot)
        };
}
