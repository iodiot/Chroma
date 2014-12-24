using System;
using Chroma;

namespace Chroma.Gui
{
  public abstract class Gui
  {
    protected readonly Core core;

    public Gui(Core core)
    {
      this.core = core;
    }

    public virtual void Update(int ticks)
    {
    }

    public virtual void Draw()
    {
    }
  }
}

