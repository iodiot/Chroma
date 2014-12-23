using System;
using System.Collections.Generic;
using Chroma.Messages;

namespace Chroma.Actors
{
  public abstract class Actor : ISubscriber
  {
    public string Handle { get; protected set; }

    protected readonly Core core;

    public Actor(Core core)
    {
      this.core = core;

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

