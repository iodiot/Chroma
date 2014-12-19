using System;
using System.Collections.Generic;

namespace Chroma.Messages
{
  enum MessageType
  {
    AddActor,
    RemoveActor,
    Text
  };

  abstract class Message
  {
    public MessageType Type { get; private set; }

    public Message(MessageType type)
    {
      Type = type;
    }
  }
}

