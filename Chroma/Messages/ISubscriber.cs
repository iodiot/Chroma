using System;

namespace Chroma.Messages
{
  public interface ISubscriber
  {
    void OnMessage(Message message, object sender);
  }
}

