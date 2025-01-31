﻿using System;
using System.Threading.Tasks;

namespace SWB.Base;

public static class ParticleExtensions
{
	public static async void PlayUntilFinished( this SceneParticles particles, TaskSource source, Action<SceneParticles> OnFrame = null )
	{
		try
		{
			while ( particles.IsValid() && !particles.Finished )
			{
				await source.Frame();

				if ( OnFrame is not null )
					OnFrame( particles );

				try
				{
					// It just throws exceptions all the time
					particles?.Simulate( Time.Delta );
				}
				catch ( Exception e )
				{
					Log.Warning( e );
				}
			}
		}
		catch ( TaskCanceledException )
		{
			// Do nothing.
		}

		particles.Delete();
	}
}
