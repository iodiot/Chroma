using System;
using System.Collections.Generic;

namespace Chroma
{
  public sealed class Timer
  {
    public int StartTick { get; private set; }
    public int StartDelay { get; private set; }
    public int Interval { get; private set; }
    public int Loops { get; private set; }
    public Action<Timer> OnTimer { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsFinished { get; private set; }

    public int ElapsedTicks { get; private set; }
    public int ElapsedLoops { get; private set; }

    public Timer(int startTick, int startDelay, int interval, int loops, Action<Timer> onTimer)
    {
      Debug.Assert(interval > 0, "Timer(): Interval <= 0");

      StartTick = startTick;
      StartDelay = startDelay;
      Interval = interval;
      Loops = loops;
      OnTimer = onTimer;

      IsActive = true;
    }

    public void Stop()
    {
      IsActive = false;
    }

    public void Update(int ticks)
    {
      if (!IsActive || IsFinished)
      {
        return;
      }

      var relativeTick = ticks - StartTick - StartDelay;
      if (relativeTick >= 0)
      {
        if (relativeTick % Interval == 0)
        {
          ++ElapsedLoops;
          OnTimer(this);
        }
      }

      ++ElapsedTicks;

      if (Loops > 0 && ElapsedLoops == Loops)
      {
        IsFinished = true;
      }
    }
  }

  public sealed class TimerManager
  {
    private readonly Core core;
    private readonly List<Timer> timers;

    private int currentTick;

    public TimerManager(Core core)
    {
      this.core = core;

      timers = new List<Timer>();
    }

    public void Update(int ticks)
    {
      currentTick = ticks;

      var timersToRemove = new List<Timer>();

      foreach (var t in timers)
      {
        t.Update(ticks);

        if (t.IsFinished)
        {
          timersToRemove.Add(t);
        }
      }

      foreach (var t in timersToRemove)
      {
        timers.Remove(t);
      }
    }

    public Timer CreateTimer(int startDelay, int interval, int loops, Action<Timer> onTimer)
    {
      var timer = new Timer(currentTick, startDelay, interval, loops, onTimer);
      timers.Add(timer);
      return timer;
    }
  }
}

