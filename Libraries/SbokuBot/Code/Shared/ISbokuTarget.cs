namespace Sandbox.Sboku.Shared;
public interface ISbokuTarget
{
    GameObject GameObject { get; }
    Vector3 WorldPosition { get; }
    bool IsEnemy { get; }
    bool IsValid { get; }
    bool IsAlive { get; }
}