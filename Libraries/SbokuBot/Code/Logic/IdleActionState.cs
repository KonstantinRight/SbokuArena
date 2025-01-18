using Sandbox.Sboku.Shared;
using System;

namespace Sandbox.Sboku.Logic;
internal class IdleActionState : StateBase, IActionState
{
    public IdleActionState(SbokuBase bot) : base(bot)
    {
    }

    public override void Think()
    {
        if (Target == null)
        {
            (ISbokuTarget Target, float SquaredDistance)? saved = null;
            foreach (var tar in Bot.Scene.GetAllComponents<ISbokuTarget>())
            {
                var dist = Bot.WorldPosition.DistanceSquared(tar.WorldPosition);
                if (tar.IsEnemy && dist <= MathF.Pow(Bot.SearchRange, 2))
                {
                    if (saved == null || dist < saved.Value.SquaredDistance)
                    {
                        saved = new(tar, dist);
                    }
                }
            }

            if (saved != null)
                Bot.Target = saved.Value.Target;
        }

        if (Bot.Target != null)
        {
            if (Bot.WorldPosition.DistanceSquared(Bot.Target.GameObject.WorldPosition) > MathF.Pow(Bot.MaxFightRange, 2))
                Bot.SetActionState<ChaseState>();
            else
                Bot.SetActionState<TacticalState>();
        }
    }

    public override void OnSet()
    {
        Bot.StopNavigating();
    }
}
