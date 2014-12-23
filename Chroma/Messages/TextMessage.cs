using System;
using Chroma.Messages;

namespace Chroma.Messages
{
  public class TextMessage : Message
  {
    public string Text { get; private set; }

    public TextMessage(string text) : base(MessageType.Text)
    {
      Text = text;
    }
  }
}

