using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chroma.Actors;
using Chroma.Gameplay;
using Chroma.Graphics;
using Chroma.Helpers;
using Chroma.Messages;
using Chroma.States;
using CoreMotion;
using Foundation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Chroma
{
  public sealed partial class Core : ISubscriber
  {
    private List<Pair<string, int>> debugMessages;

    private Dictionary<string, string> debugWatches;
    private Dictionary<string, int> debugWatchTtls;
    private List<string> debugWatchesToRemove;

    private Dictionary<string, float> liveTuners;
    private Dictionary<string, float> liveTuneSteps;
    private Dictionary<string, Rectangle> liveTuneMinus;
    private Dictionary<string, Rectangle> liveTunePlus;

    private void InitDebugTools() {
      debugMessages = new List<Pair<string, int>>();

      debugWatches = new Dictionary<string, string>();
      debugWatchTtls = new Dictionary<string, int>();
      debugWatchesToRemove = new List<string>();

      liveTuners = new Dictionary<string, float>();
      liveTuneSteps = new Dictionary<string, float>();
      liveTuneMinus = new Dictionary<string, Rectangle>();
      liveTunePlus = new Dictionary<string, Rectangle>();
    }

    // Debug messages

    public void DebugMessage(string message)
    {
      if (!Settings.DrawDebugMessages)
        return;

      debugMessages.Insert(0, new Pair<string, int>(message, 200));
    }

    public void DebugMessage(bool condition, string message) 
    {
      if (condition)
        DebugMessage(message);
    }

    // Debug watches

    public void DebugWatch(string watchName, string watchValue, int ttl = 10)
    {
      if (!Settings.DrawDebugMessages)
        return;
      debugWatches[watchName] = watchValue;
      debugWatchTtls[watchName] = ttl;
    }

    public void DebugWatch(Object obj, string watchName, string watchValue, int ttl = 10)
    {
      if (!Settings.DrawDebugMessages)
        return;

      DebugWatch(
        obj.GetType().Name + " " + String.Format("{0,3:X}", obj.GetHashCode() % 1000) + ((watchName != "") ? " " + watchName : ""), 
        watchValue, ttl);
    }

    public void RemoveDebugWatch(string watchName)
    {      
      if (!Settings.DrawDebugMessages)
        return;
      debugWatches.Remove(watchName);
      debugWatchTtls.Remove(watchName);
    }

    // Live tuners

    public float LiveTune(string name, float value, float step = 0.1f) {      
      if (!Settings.DrawDebugMessages)
        return value;
      if (liveTuners.ContainsKey(name))
      {
        return liveTuners[name];
      }
      else
      {
        liveTuners.Add(name, value);
        liveTuneSteps.Add(name, step);
        return value;
      }
    }

    public int LiveTune(string name, int value, int step = 1) {      
      if (!Settings.DrawDebugMessages)
        return value;
      if (liveTuners.ContainsKey(name))
      {
        return (int)liveTuners[name];
      }
      else
      {
        liveTuners.Add(name, value);
        liveTuneSteps.Add(name, step);
        return value;
      }
    }

    private void HandleDebugInput() {
      if (!Settings.DrawDebugMessages || liveTuners.Count == 0)
        return;
      foreach (TouchLocation touch in TouchState)
      {
        if (touch.State != TouchLocationState.Pressed)
          continue;
        foreach (var button in liveTuneMinus)
        {
          if (button.Value.Contains(touch.Position))
            liveTuners[button.Key] -= liveTuneSteps[button.Key];

          if (liveTunePlus[button.Key].Contains(touch.Position))
            liveTuners[button.Key] += liveTuneSteps[button.Key];
        }
      }
    }

    //==================================

    private void UpdateDebugMessages()
    {
      if (Settings.DrawDebugMessages)
      {
        // Messages
        foreach (var message in debugMessages)
        {
          message.B--;
        }
        debugMessages.RemoveAll(m => m.B == 0);

        // Watches
        foreach (var key in debugWatchTtls.Keys.ToList())
        {
          debugWatchTtls[key] -= 1;
          if (debugWatchTtls[key] == 0)
            debugWatchesToRemove.Add(key);
        }
        foreach (var watchName in debugWatchesToRemove) {
          debugWatches.Remove(watchName);
          debugWatchTtls.Remove(watchName);
        }
        debugWatchesToRemove.Clear();
      }
    }

    private void DrawDebugMessages()
    {
      if (Settings.DrawDebugMessages)
      {
        // Messages
        float i = -5;
        foreach (var message in debugMessages) {
          i += 10;
          Renderer["fg", 1000].DrawTextS(
            message.A,
            new Vector2(Renderer.ScreenWidth - 150, i),
            Color.White * 0.5f * ((float)message.B / 200)
          );
        }

        // Watches
        i = -2;
        var maxWidth = 0f;
        foreach (var watch in debugWatches) {
          i += 7;
          var w1 = Renderer["fg", 1000].DrawTextS(
            watch.Key + ": ",
            new Vector2(5, i),
            Color.Lime * 0.6f,
            0.66f
          );
          var w2 = Renderer["fg", 1000].DrawTextS(
            watch.Value,
            new Vector2(5 + w1, i),
            Color.White * 0.5f,
            0.66f
          );
          maxWidth = Math.Max(w1 + w2, maxWidth);
        }
        Renderer["fg", 999].DrawRectangleS(
          new Rectangle(0, 0, (int)maxWidth + 10, 7 * debugWatches.Count + 7), 
          Color.Black * 0.5f
        );
      }
    }

    private void DrawLiveTuners() {
      if (!Settings.DrawDebugMessages || liveTuners.Count == 0)
        return;

      var y = Renderer.ScreenHeight - 80;
      var x = 120;
      var maxWidth = 0f;
      const int buttonSize = 16;
      foreach (var tuner in liveTuners)
      {
        var w1 = Renderer["fg", 1000].DrawTextS(
          tuner.Key + ":",
          new Vector2(x, y + (buttonSize - 8) / 2),
          Color.Lime * 0.6f,
          0.66f
        );
        var w2 = Renderer["fg", 1000].DrawTextS(
          String.Format("{0:0.00}", tuner.Value),
          new Vector2(x + w1 + 5, y + (buttonSize - 8) / 2),
          Color.White * 0.5f,
          0.66f
        );

        liveTuneMinus[tuner.Key] = new Rectangle((int)(x + w1 + 5 + w2 + 5), (int)y - 3, buttonSize, buttonSize);
        Renderer["fg", 1000].DrawRectangleS(
          liveTuneMinus[tuner.Key], 
          Color.Red * 0.2f
        );
        liveTunePlus[tuner.Key] = new Rectangle((int)(x + w1 + 5 + w2 + 5 + buttonSize + 1), (int)y - 3, buttonSize, buttonSize);
        Renderer["fg", 1000].DrawRectangleS(
          liveTunePlus[tuner.Key], 
          Color.Lime * 0.2f
        );

        maxWidth = Math.Max(w1 + 5 + w2 + 5 + buttonSize * 2 + 1, maxWidth);
        y += buttonSize + 1;
      }
      Renderer["fg", 999].DrawRectangleS(
        new Rectangle(x - 5, (int)Renderer.ScreenHeight - 85, (int)maxWidth + 9, (buttonSize + 1) * liveTuners.Count + 3), 
        Color.Black * 0.5f
      );
    }

  }
}

