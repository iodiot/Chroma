using System;
using Chroma.Actors;

namespace Chroma.Messages
{
  class AddActorMessage : Message
  {
    public Actor Actor { get; private set; }

    public AddActorMessage(Actor actor) : base(MessageType.AddActor)
    {
      Actor = actor;
    }
  }
}

