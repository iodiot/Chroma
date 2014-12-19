using System;
using Chroma;

namespace Chroma.Ui
{
  abstract class Ui
  {
    protected readonly Core core;

    public Ui(Core core)
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

