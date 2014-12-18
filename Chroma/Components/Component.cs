using System;
using Chroma.Actors;

namespace Chroma.Components
{
  enum ComponentType 
  {
    Body,
    Animation
  }

  abstract class Component
  {
    public ComponentType Type { get; private set; }
    protected Actor actor;

    public Component(ComponentType type, Actor actor)
    {
      Type = type;
    }

    public virtual void Update(int ticks)
    {
    }

    public void Draw()
    {
    }
  }
}

