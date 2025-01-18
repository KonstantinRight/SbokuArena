using SWB.Base.UI;
using SWB.Player;
using System;

namespace SWB.Base;

public partial class Weapon
{
	public ScreenPanel ScreenPanel { get; set; }
	public PanelComponent RootPanel { get; set; }

	private CustomizationMenu customizationMenu;

	/// <summary>Override this if you want custom UI elements</summary>
	public virtual void CreateUI()
	{
        if (IsOwnerBot)
            return;

        ScreenPanel = Components.Create<ScreenPanel>();
		ScreenPanel.Opacity = 1;
		ScreenPanel.ZIndex = 1;

		var rootPanel = Components.Create<RootWeaponDisplay>();
		rootPanel.Weapon = this;
		RootPanel = rootPanel;

		// Attachments (HUD)
		RootPanel.OnComponentStart += () =>
		{
			Attachments.ForEach( ( att ) =>
			{
				if ( att.Equipped && !att.CreatedUI )
					att.CreateHudElements();
			} );
		};
	}

	public virtual void DestroyUI()
	{
		ScreenPanel?.Destroy();
		RootPanel?.Destroy();
	}

	void BroadcastUIEvent( string name, object value )
	{
		if ( RootPanel is null ) return;

		foreach ( var panel in RootPanel.Panel.Children )
		{
			panel.CreateEvent( name, value );
		}
	}

	void OpenCustomizationMenu()
	{
		customizationMenu = new CustomizationMenu( this );
		try
		{
			RootPanel.Panel.AddChild( customizationMenu );
		}
		catch (Exception ex)
		{
			Log.Warning(ex);
		}
	}

	void CloseCustomizationMenu()
	{
		customizationMenu?.Delete( true );
	}
}
