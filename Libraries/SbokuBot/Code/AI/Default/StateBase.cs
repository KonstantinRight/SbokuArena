using Sandbox.Sboku;
using Sandbox.Shared;

namespace Sandbox.AI.Default;
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
