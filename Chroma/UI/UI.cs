using System;
using Chroma;

namespace Chroma.UI
{
  abstract class UI
  {
    protected readonly Core core;

    public UI(Core core)
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

