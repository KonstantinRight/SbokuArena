using Sandbox.Sboku.Arena;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Shared;

namespace SWB.HUD;
internal class TimerDisplay : Panel
{
    IPlayerBase player;
    RoundManager roundManager;
    Label label;

    public TimerDisplay(IPlayerBase player, RoundManager roundManager)
    {
        this.player = player;
        this.roundManager = roundManager;

        StyleSheet.Load("/swb_hud/TimerDisplay.cs.scss");

        var panel = Add.Panel("box");
        panel.Add.Label("Timer", "name");
        label = panel.Add.Label("", "timer");
    }

    public override void Tick()
    {
        var isAlive = player.IsAlive;
        var hide = !isAlive || roundManager.Timer == 0;
        SetClass("hide", hide);

        if (hide) return;

        if (label is not null)
        {
            label.Text = roundManager.Timer.ToString();
            label.Style.FontColor = Color.White;
        }
    }
}
