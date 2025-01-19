using Sandbox.Sboku;
using Sandbox.Sboku.Shared;

namespace Sandbox.AI.Default;
public class SbokuParent
{
    protected SbokuBase Bot { get; }
    protected Scene Scene => Bot.Scene;
    protected SbokuSettings Settings => Bot.Settings;
    protected ISbokuTarget Target => Bot.Target;
    protected ISbokuWeapon Weapon => Bot.Weapon;

    /// <summary>
    /// Get squared distance to target. If turget is null, we'll get NRE.
    /// </summary>
    protected float SquaredDistanceToTarget => Bot.WorldPosition.DistanceSquared(Target.GameObject.WorldPosition);

    protected SbokuParent(SbokuBase bot)
    {
        Bot = bot;
    }
}
