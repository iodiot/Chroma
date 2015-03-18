using System;
using Chroma.Actors;

namespace Chroma.Messages
{
  public class AddActorMessage : Message
  {
    public readonly Actor Actor;

    public AddActorMessage(Actor actor) : base(MessageType.AddActor)
    {
      Actor = actor;
    }
  }
}

