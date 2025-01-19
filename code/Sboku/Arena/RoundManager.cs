using Sandbox.Events;
using SWB.Demo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.Sboku.Arena;

[Group("Sboku Arena")]
public sealed class RoundManager : Component,
	IGameEventHandler<RoundManager.UpgradeScreenClosed>,
	IGameEventHandler<RoundManager.GameOver>
{
	[Property]
	public int EnemiesPerSpawner { get; set; } = 3;
	[Property]
	public int StartCountdown { get; set; } = 5;
    [Property]
    public int EndCountdown { get; set; } = 5;
    [Property]
    public int BotCap { get; set; } = 3;
    [Property]
    public int TotalRounds { get; set; } = 10;

    [Sync]
	public int RoundNumber { get; set; } = 1;
	[Sync]
	public int BotsNumber { get; private set; }
	[Sync]
	public int Timer { get; private set; }

	private TimeSince timer = new();
	private Action callback;

	private bool isRoundStarted = false;
	private List<Spawner> spawners;
    
	[Rpc.Host]
    protected override void OnAwake()
    {
		spawners = Scene.GetAllComponents<Spawner>().ToList();
    }

    [Rpc.Host]
    protected override void OnFixedUpdate()
    {
        BotsNumber = Scene.GetAllComponents<SbokuBase>().Count();
        if (isRoundStarted && BotsNumber == 0 && AreSpawnersDone())
		{
			isRoundStarted = false;
            SetCallback(EndCountdown, FinishRound);
        }

        if (timer > 1f)
		{
			OnTimer();
			timer = 0f;
		}
    }

	private void OnTimer()
	{
		if (Timer > 0)
		{
			Timer--;
            if (Timer == 0)
			{
				OnTimerExpire();
			}
		}
	}
	private void OnTimerExpire()
	{
		if (callback != null)
		{
			callback();
		}
		else
		{
			Log.Error("Callback is null!");
		}
	}

    private bool AreSpawnersDone()
    {
		foreach (var spawner in spawners)
		{
			if (!spawner.IsDone())
				return false;
		}

		return true;
    }

    private void SetCallback(int time,  Action callback)
	{
		Timer = time;
		this.callback = callback;
	}

    [Rpc.Host]
    private void StartRound()
	{
		if (isRoundStarted) return;

		foreach (var spawner in spawners)
		{
			for (var i = 0; i < EnemiesPerSpawner; i++)
				spawner.RequestSpawn();
		}

		isRoundStarted = true;
    }
	[Rpc.Host]
	public void RemoveEntities()
	{
        foreach (var ad in Scene.GetComponentsInChildren<SWBAdapter>(includeDisabled: true))
            ad.GameObject.Destroy();
    }

	[Rpc.Broadcast]
	private void FinishRound()
	{
		isRoundStarted = false;
		RoundNumber++;

		RemoveEntities();

		foreach (var ply in Scene.GetAllComponents<DemoPlayer>())
		{
			ply.Respawn();
			ply.GetComponent<UpgradeHolder>().FreePoints++;
		}

		if (RoundNumber <= TotalRounds)
		{
			Scene.Dispatch<OpenUpgradeScreen>(new());
		}
		else
		{
            Scene.Dispatch<Victory>(new());
        }
    }

    public void OnGameEvent(UpgradeScreenClosed eventArgs)
    {
		if (!isRoundStarted)
			SetCallback(StartCountdown, StartRound);
    }

	[Rpc.Broadcast]
    public void OnGameEvent(GameOver eventArgs)
    {
		isRoundStarted = false;
		spawners.ForEach(x => x.ClearQueue());
		RemoveEntities();

        foreach (var pl in Scene.GetComponentsInChildren<DemoPlayer>())
		{
	        pl.Respawn();
            pl.CreateFailScreen();
		}
    }

    public record OpenUpgradeScreen() : IGameEvent;
	public record UpgradeScreenClosed() : IGameEvent;
	public record GameOver() : IGameEvent;
	public record Victory() : IGameEvent;
}
