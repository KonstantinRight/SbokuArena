using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWB.Demo;

[Group( "SWB" )]
[Title( "Demo NetworkManager" )]
public class DemoNetworkManager : Component, Component.INetworkListener
{
	[Property] public PrefabScene PlayerPrefab { get; set; }

	protected override Task OnLoad()
	{
		if ( !Networking.IsActive )
			Networking.CreateLobby( new() );

		return base.OnLoad();
	}

	private List<Connection> connections = new();
	// Called on host
	void INetworkListener.OnActive( Connection connection )
	{
		if (connections.Contains(connection))
		{
			Log.Warning("Connection is doubled. Somehow.");
			return;
		}
		connections.Add(connection);

		var playerGO = PlayerPrefab.Clone();
		playerGO.Name = "Player";
		playerGO.NetworkSpawn( connection );
	}
}
