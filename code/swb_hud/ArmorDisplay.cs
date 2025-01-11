using Sandbox.Sboku.Arena;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Shared;

namespace SWB.HUD;

public class ArmorDisplay : Panel
{
	IPlayerBase player;
	Label armorLabel;

	public ArmorDisplay( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/ArmorDisplay.cs.scss" );

		Add.Label( "armor", "name" );
		armorLabel = Add.Label( "", "armor" );
	}

	public override void Tick()
	{
		var isAlive = player.IsAlive;
		SetClass( "hide", !isAlive );

		if ( !isAlive ) return;

		if ( armorLabel is not null )
		{
			armorLabel.Text = player.GameObject.GetComponent<ArmorClass>().Class.ToString();
			armorLabel.Style.FontColor = Color.Cyan;
		}
	}
}
