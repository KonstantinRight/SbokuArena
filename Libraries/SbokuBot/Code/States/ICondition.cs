namespace Sandbox.Sboku.States;
public interface ICondition
{
    bool If();
    void Then();
    /// <summary>
    /// Should we stop evaluating other conditions if the condition is true
    /// </summary>
    /// <returns></returns>
    bool IsTerminal();
}
