using Sandbox.Sboku.Shared;
using System;

namespace Sandbox.Sboku.Testing;
internal class Mover : Component, ISbokuTarget
{
    [Property]
    public float Speed { get; set; } = 50f;
    [Property]
    public float Offset { get; set; } = 100;
    [Property]
    public float Amplitude { get; set; } = 10f;
    [Property]
    public CharacterController CharacterController { get; set; }

    public bool IsEnemy => true;
    public bool IsAlive => true;

    private float targetX = 0;
    private float curX = 0;
    private Vector3 initial;

    protected override void OnStart()
    {
        initial = GameObject.WorldPosition;
        targetX += initial.x + Offset;
    }

    protected override void OnUpdate()
    {
        curX = GameObject.WorldPosition.x;
        if (targetX < 0)
        {
            if (targetX < curX)
            {
                GameObject.WorldPosition += new Vector3(-Time.Delta * Speed, 0, 0);
            }
            else
            {
                targetX = initial.x + Offset * 2;
            }
        }
        else
        {
            if (targetX > curX)
            {
                GameObject.WorldPosition += new Vector3(Time.Delta * Speed, 0, 0);
            }
            else
            {
                targetX = initial.x + -Offset * 2;
            }
        }

        GameObject.WorldPosition = new Vector3(GameObject.WorldPosition.x, GameObject.WorldPosition.y, initial.z + MathF.Sin(Time.Now * 4) * Amplitude);
    }
}
