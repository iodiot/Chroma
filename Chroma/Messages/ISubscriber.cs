using System;

namespace Chroma.Messages
{
  interface ISubscriber
  {
    void OnMessage(Message message, object sender);
  }
}

