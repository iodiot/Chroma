using System;
using Chroma.Messages;

namespace Chroma
{
  class TextMessage : Message
  {
    public string Text { get; private set; }

    public TextMessage(string text) : base(MessageType.Text)
    {
      Text = text;
    }
  }
}

