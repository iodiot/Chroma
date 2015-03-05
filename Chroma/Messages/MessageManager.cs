using System;
using System.Collections.Generic;

namespace Chroma.Messages
{
  public sealed class MessageManager
  {
    private sealed class MessageDesc
    {
      public Message Message;
      public object Sender;
      public string Handle; 
      public int Delay;
    };

    private readonly Core core;
    private readonly Dictionary<MessageType, List<ISubscriber>> subscribers;
    private readonly Dictionary<string, ISubscriber> handles;
    private readonly List<MessageDesc> messagesDesc;

    public MessageManager(Core core)
    {
      this.core = core;
      subscribers = new Dictionary<MessageType, List<ISubscriber>>();
      handles = new Dictionary<string, ISubscriber>();
      messagesDesc = new List<MessageDesc>();
    }

    public void Load()
    {
    }

    public void Unload()
    {
      Debug.Assert(handles.Count == 0, "MessageManager.Unload() : Handles list is not empty");
    }

    public void Update(int ticks)
    {
      SendAll();
    }

    public void Subscribe(MessageType messageType, ISubscriber subscriber)
    {
      if (!subscribers.ContainsKey(messageType))
      {
        subscribers[messageType] = new List<ISubscriber>();
      }

      subscribers[messageType].Add(subscriber);
    }

    public void Send(Message message, object sender, int delay = 0)
    {
      Debug.Print(String.Format("{0} {1}", message.ToString(), sender.ToString()));
      messagesDesc.Add(new MessageDesc() { Message = message, Sender = sender, Handle = String.Empty, Delay = delay });
    }

    private void SendAll()
    {
      var delayed = new List<MessageDesc>();

      foreach (var desc in messagesDesc)
      {
        if (desc.Delay > 0)
        {
          desc.Delay -= 1;
          delayed.Add(desc);
          continue;
        }

        // send by type
        if (desc.Handle == String.Empty)
        {
          if (subscribers.ContainsKey(desc.Message.Type))
          {
            foreach (var s in subscribers[desc.Message.Type])
            {
              s.OnMessage(desc.Message, desc.Sender);
            }
          }
        }

        // send by handle
        else
        {
          if (handles.ContainsKey(desc.Handle))
          {
            handles[desc.Handle].OnMessage(desc.Message, desc.Sender);
          }
        }
      }

      messagesDesc.Clear();
      messagesDesc.AddRange(delayed);
    }

    public void SubscribeByHandle(string handle, ISubscriber subscriber)
    {
      handles.Add(handle, subscriber);
    }

    public void UnsubscribeByHandle(string handle)
    {
      Debug.Assert(handles[handle] != null, String.Format("MessageManager.UnsubscribeByHandle() : Subscriber {0} is missing", handle));

      handles.Remove(handle);
    }

    public void SendByHandle(string handle, Message message, object sender, int delay = 0)
    {
      messagesDesc.Add(new MessageDesc() { Message = message, Sender = sender, Handle = handle, Delay = delay });
    }
  }
}

