using System;
using Chroma.Actors;

namespace Chroma.Messages
{
  public class RemoveActorMessage : Message
  {
    public readonly Actor Actor;

    public RemoveActorMessage(Actor actor) : base(MessageType.RemoveActor)
    {
      Actor = actor;
    }
  }
}

