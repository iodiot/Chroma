using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Messages;

namespace Chroma.Actors
{
  public abstract class Actor : ISubscriber
  {
    public string Handle { get; protected set; }
    public Vector2 Position;
    public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
    public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }

    protected readonly Core core;

    public Actor(Core core, Vector2 position)
    {
      this.core = core;

      Position = position;

      Handle = String.Empty;
    }
      
    public virtual void Load()
    {
      if (Handle != String.Empty)
      {
        core.MessageManager.SubscribeByHandle(Handle, this);
      }
    }

    public virtual void Unload()
    {
      if (Handle != String.Empty)
      {
        core.MessageManager.UnsubscribeByHandle(Handle);
      }
    }

    public virtual void Update(int ticks)
    {
    }

    public virtual void Draw()
    {
    }

    public virtual void OnMessage(Message message, object sender)
    {
    }
  }
}

