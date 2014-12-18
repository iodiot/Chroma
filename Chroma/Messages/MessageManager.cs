using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chroma.Messages
{
  sealed class MessageManager
  {
    private readonly Core core;
    private readonly Dictionary<MessageType, List<ISubscriber>> subscribers;
    private readonly Dictionary<string, ISubscriber> handles;

    public MessageManager(Core core)
    {
      this.core = core;
      subscribers = new Dictionary<MessageType, List<ISubscriber>>();
      handles = new Dictionary<string, ISubscriber>();
    }

    public void Load()
    {
    }

    public void Unload()
    {
      Debug.Assert(handles.Count == 0, "~MessageManager() - Handles list is not empty");
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
      if (subscribers.ContainsKey(message.Type))
      {
        foreach (var s in subscribers[message.Type])
        {
          s.OnMessage(message, sender);
        }
      }
    }

    public void SubscribeByHandle(string handle, ISubscriber subscriber)
    {
      handles.Add(handle, subscriber);
    }

    public void UnsubscribeByHandle(string handle)
    {
      Debug.Assert(handles[handle] != null, String.Format("MessageManager.UnsubscribeByHandle() - Subscriber {0} is missing", handle));

      handles.Remove(handle);
    }

    public void SendByHandle(string handle, Message message, object sender)
    {
      if (handles.ContainsKey(handle))
      {
        handles[handle].OnMessage(message, sender);
      }
    }
  }
}

