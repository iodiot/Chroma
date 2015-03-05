using System;
using System.Collections.Generic;

namespace Chroma.Messages
{
  public enum MessageType
  {
    AddActor,
    RemoveActor,
    CoreEvent,
    Text
  };

  public abstract class Message
  {
    public MessageType Type { get; private set; }

    public Message(MessageType type)
    {
      Type = type;
    }
  }
}

