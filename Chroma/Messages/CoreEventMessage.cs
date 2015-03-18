using System;
using Chroma.Actors;

namespace Chroma.Messages
{
  public enum CoreEvent 
  {
    StartGame,
    GameOver,
    ResetGame
  }

  public class CoreEventMessage : Message
  {
    public readonly CoreEvent CoreEvent;

    public CoreEventMessage(CoreEvent coreEvent) : base(MessageType.CoreEvent)
    {
      CoreEvent = coreEvent;
    }
  }
}
