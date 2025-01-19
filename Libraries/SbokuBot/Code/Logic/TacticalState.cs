using System.Linq;

namespace Sandbox.Sboku.Logic;
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
        // TODO: random offset
        // TODO: avoid paths that on lines of fire
        for (float angle = 0; angle < 360; angle += Settings.CoverScanAngle)
        {
            Vector3 direction = Rotation.FromYaw(angle).Forward;
            var h = Bot.Character.Height / 2;
            var trace = Scene.Trace.Ray(botPos + h, botPos + h + direction * Bot.MaxFightRange)
                                   .IgnoreGameObjectHierarchy(Bot.GameObject)
                                   .IgnoreGameObjectHierarchy(Target.GameObject)
                                   .Run();

            if (Settings.ShowDebugOverlay)
                Bot.Scene.DebugOverlay.Line(trace.StartPosition, trace.EndPosition, Color.Magenta, 3);

            if (trace.Hit && trace.GameObject != null)
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
}
