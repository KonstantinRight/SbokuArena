using Sandbox.Sboku.States;

namespace Sandbox.Sboku.Logic;
public abstract class StateBase : SbokuParent, ISbokuState
{
    protected StateBase(SbokuBase bot) : base(bot)
    {
    }

    public virtual void Think()
    {
    }

    public virtual void OnSet()
    {
    }

    public virtual void OnUnset()
    {
    }
}
