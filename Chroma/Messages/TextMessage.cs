using System;
using Chroma.Messages;

namespace Chroma.Messages
{
  public class TextMessage : Message
  {
    public readonly string Text;

    public TextMessage(string text) : base(MessageType.Text)
    {
      Text = text;
    }
  }
}

