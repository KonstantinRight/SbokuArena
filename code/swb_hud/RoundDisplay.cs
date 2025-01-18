using Sandbox.Sboku.Arena;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Shared;

namespace SWB.HUD;
internal class RoundDisplay : Panel
{
    IPlayerBase player;
    RoundManager roundManager;
    Label label;

    public RoundDisplay(IPlayerBase player, RoundManager roundManager)
    {
        this.player = player;
        this.roundManager = roundManager;

        StyleSheet.Load("/swb_hud/RoundDisplay.cs.scss");

        Add.Label("Round", "name");
        label = Add.Label("", "Round");
    }

    public override void Tick()
    {
        var isAlive = player.IsAlive;
        SetClass("hide", !isAlive);

        if (!isAlive) return;

        if (label is not null)
        {
            label.Text = $"{roundManager.RoundNumber}/{roundManager.TotalRounds}";
            label.Style.FontColor = Color.White;
        }
    }
}
