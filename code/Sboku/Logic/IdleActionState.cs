using SWB.Player;
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
            foreach (var ply in Bot.Scene.GetAllComponents<PlayerBase>())
            {
                if (Bot.WorldPosition.DistanceSquared(ply.WorldPosition) <= MathF.Pow(Bot.SearchRange, 2))
                {
                    Bot.Target = ply;
                }
            }
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
