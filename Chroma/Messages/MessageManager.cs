using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chroma.Messages
{
  sealed class MessageManager
  {
    private struct MessageDesc
    {
      public Message Message;
      public object Sender;
      public string Handle; 
    };

    private readonly Core core;
    private readonly Dictionary<MessageType, List<ISubscriber>> subscribers;
    private readonly Dictionary<string, ISubscriber> handles;
    private readonly Queue<MessageDesc> messageDescQueue;

    public MessageManager(Core core)
    {
      this.core = core;
      subscribers = new Dictionary<MessageType, List<ISubscriber>>();
      handles = new Dictionary<string, ISubscriber>();
      messageDescQueue = new Queue<MessageDesc>();
    }

    public void Load()
    {
    }

    public void Unload()
    {
      Debug.Assert(handles.Count == 0, "~MessageManager() : Handles list is not empty");
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

    public void Send(Message message, object sender)
    {
      messageDescQueue.Enqueue(new MessageDesc() { Message = message, Sender = sender, Handle = String.Empty });
    }

    private void SendAll()
    {
      foreach (var desc in messageDescQueue)
      {
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
        else
        {
          if (handles.ContainsKey(desc.Handle))
          {
            handles[desc.Handle].OnMessage(desc.Message, desc.Sender);
          }
        }
      }
      messageDescQueue.Clear();
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

    public void SendByHandle(string handle, Message message, object sender)
    {
      messageDescQueue.Enqueue(new MessageDesc() { Message = message, Sender = sender, Handle = handle });
    }
  }
}

