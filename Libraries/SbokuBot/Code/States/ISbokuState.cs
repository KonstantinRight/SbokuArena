namespace Sandbox.Sboku.States;
/// <summary>
/// General interface for any state
/// </summary>
public interface ISbokuState
{
    public void Think();
    public void OnSet();
    public void OnUnset();
}
