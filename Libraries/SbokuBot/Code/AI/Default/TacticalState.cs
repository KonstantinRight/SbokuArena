using Sandbox.Sboku;
using Sandbox.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.AI.Default;
internal class TacticalState : StateBase, IActionState
{
    public TacticalState(SbokuBase bot) : base(bot)
    {
    }

    public override void Think()
    {
        if (!Bot.IsNavigating)
        {
            // Repeat
            Bot.SetActionState<TacticalState>();
            return;
        }
    }

    public override void OnSet()
    {
        FindCover();
    }

    private void FindCover()
    {
        Vector3 tarPos = Target.GameObject.WorldPosition;
        Vector3 botPos = Bot.WorldPosition;
        
        var startAngle = Game.Random.Next(0, 360 + 1);
        for (float angle = startAngle; angle < startAngle + 360; angle += Settings.CoverScanAngle)
        {
            Vector3 direction = Rotation.FromYaw(angle).Forward;
            var h = Bot.Character.Height / 2;
            var trace = Scene.Trace.Ray(botPos + h, botPos + h + direction * Bot.MaxFightRange)
                                   .IgnoreGameObjectHierarchy(Bot.GameObject)
                                   .IgnoreGameObjectHierarchy(Target.GameObject)
                                   .Run();
            Scene.GetAllComponents<ISbokuBot>();
            if (Settings.ShowDebugOverlay)
                Bot.Scene.DebugOverlay.Line(trace.StartPosition, trace.EndPosition, Color.Magenta, 3);

            if (trace.Hit && trace.GameObject != null && !PathCrossesFire(botPos, trace.EndPosition))
            {
                var thru = Scene.Trace.Ray(trace.EndPosition, trace.EndPosition + direction * 50).IgnoreGameObjectHierarchy(Scene).Run();
                var pos = Scene.NavMesh.GetClosestPoint(thru.EndPosition);
                if (pos is not Vector3 vect)
                    continue;

                var potentialPath = Bot.Scene.NavMesh.GetSimplePathSafe(Bot.WorldPosition, vect);
                if (potentialPath.Any())
                {
                    if (Settings.ShowDebugOverlay)
                    {
                        Scene.DebugOverlay.Sphere(new Sphere(trace.EndPosition, 15), Color.Orange, 3);
                        Scene.DebugOverlay.Sphere(new Sphere(vect, 15), Color.Red, 3);
                    }
                    var path = Scene.NavMesh.GetSimplePathSafe(Bot.WorldPosition, vect);
                    if (path.Any())
                    {
                        Bot.MoveTo(path);
                        return;
                    }
                }
            }
        }

        var rand = Scene.NavMesh.GetRandomPoint(Target.GameObject.WorldPosition, Bot.MaxFightRange);
        // If not, we'll try again on the next think
        if (rand is Vector3 point)
        {
            Bot.MoveTo(point);
        }
    }

    private bool PathCrossesFire(Vector3 startPos, Vector3 endPos)
    {
        foreach (var otherBot in Scene.GetAllComponents<SbokuBase>().Where(x => x.IsValid && x.IsActiveCombatState<ShootState>()))
        {
            if (otherBot == Bot)
                continue; 

            Vector3 botToTargetStart = otherBot.WorldPosition;
            Vector3 botToTargetEnd = otherBot.Target.GameObject.WorldPosition;

            var fireLine = Scene.Trace.Ray(botToTargetStart, botToTargetEnd)
                                      .IgnoreGameObjectHierarchy(otherBot.GameObject)
                                      .IgnoreGameObjectHierarchy(otherBot.Target.GameObject)
                                      .Run();

            if (!fireLine.Hit)
                continue;

            if (PathsIntersect(startPos.x, startPos.y, endPos.x, endPos.y,
                               botToTargetStart.x, botToTargetStart.y, botToTargetEnd.x, botToTargetEnd.y))
            {
                return true;
            }
        }
        return false;
    }

    private bool PathsIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
    {
        float d = (x4 - x3) * (y2 - y1) - (y4 - y3) * (x2 - x1);
        if (d == 0) return false;

        float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / d;
        float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / d;

        return uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1;
    }
}
