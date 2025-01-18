using System;

namespace Sandbox.Sboku.Logic;
internal class ChaseState : StateBase, IActionState
{
    private Vector3 savedTargetPos = Vector3.Zero;
    private bool NeedToRecalculatePath 
        => MathF.Pow(Bot.DistanceToRecalucaltePath, 2) >= Bot.Target.GameObject.WorldPosition.DistanceSquared(savedTargetPos);

    public ChaseState(SbokuBase bot) : base(bot)
    {
    }

    public override void Think()
    {
        if (Scene.Trace.FromTo(Bot.EyePos, Target.GameObject.WorldPosition + Bot.HeightToAimAt)
                       .IgnoreGameObjectHierarchy(Bot.GameObject)
                       .Run().GameObject?.Parent == Target.GameObject
            && SquaredDistanceToTarget < MathF.Pow(Bot.MinFightRange, 2))
        {
            Bot.StopNavigating();
            Bot.SetActionState<TacticalState>();
            return;
        }
        else if (!Bot.IsNavigating || NeedToRecalculatePath)
        {
            savedTargetPos = Target.GameObject.WorldPosition;
            Bot.MoveTo(savedTargetPos);
            if (!Bot.IsNavigating)
                return;
        }
    }
}
