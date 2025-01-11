using SWB.Shared;

namespace Sandbox.Sboku.Logic;
public class SbokuParent
{
    protected SbokuBase Bot { get; }
    protected Scene Scene => Bot.Scene;
    protected SbokuSettings Settings => Bot.Settings;
    protected IPlayerBase Target => Bot.Target;
    protected IPlayerBase Adapter => Bot.GetComponent<SWBAdapter>();
    /// <summary>
    /// Get squared distance to target. If turget is null, we'll get NRE
    /// </summary>
    protected float SquaredDistanceToTarget => Bot.WorldPosition.DistanceSquared(Target.GameObject.WorldPosition);

    protected SbokuParent(SbokuBase bot)
    {
        Bot = bot;
    }
}
