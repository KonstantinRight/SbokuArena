using System;
using System.Collections.Generic;
using Sandbox.Internal;

namespace Sandbox.Sboku;
internal class TimerHelper : IUpdateSubscriber
{
    private static Dictionary<object, Entry> events = new();
    private record Entry(Action Action, float Period)
    {
        public TimeSince Since { get; set; } = new TimeSince();
    }
    private object Add(Action action, float period)
    {
        var handler = new object();
        events.Add(handler, new(action, period));
        return handler;
    }

    /// <summary>
    /// Runs action every given period of time in seconds
    /// </summary>
    /// <param name="time"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public object Every(float time, Action action)
        => Add(action, time);

    /// <summary>
    /// This method allows you to remove an action from the timer by utilizing the handler previously retrieved through the TimerHelper.
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public object Remove(object handler)
        => events.Remove(handler);

    public void OnUpdate()
    {
        foreach (var entry in events)
        {
            var val = entry.Value;
            if (val.Since > val.Period)
            {
                val.Since = 0f;
                val.Action();
            }
        }
    }
}
