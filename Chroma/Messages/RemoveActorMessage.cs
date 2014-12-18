﻿using System;
using Chroma.Actors;

namespace Chroma.Messages
{
  class RemoveActorMessage : Message
  {
    public Actor Actor { get; private set; }

    public RemoveActorMessage(Actor actor) : base(MessageType.RemoveActor)
    {
      Actor = actor;
    }
  }
}
