using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Chroma.Helpers;
using Chroma.Actors;
using Chroma.Messages;
using Chroma.States;
using Chroma.Graphics;
using Chroma.Gameplay;
using CoreMotion;
using Foundation;
using System.Linq;

namespace Chroma
{
  public sealed class Core : ISubscriber
  {
    private readonly List<Pair<string, int>> debugMessages;
    private readonly Dictionary<string, string> debugWatches;
    private readonly Dictionary<string, int> debugWatchTtls;
    private readonly List<string> debugWatchesToRemove;

    public SpriteManager SpriteManager { get; private set; }
    public Renderer Renderer { get; private set; }
    public ContentManager Content { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public MessageManager MessageManager { get; private set; }
    public TimerManager TimerManager { get; private set; }
    private CMMotionManager MotionManager;

    public float DeviceTilt { get; private set; }

    public GraphicsDevice GraphicsDevice;

    private readonly Stack<State> states;

    private FrameCounter frameCounter;
    public int Ticks { get; private set; }

    private readonly Stopwatch stopwatch;

    public ProfileData ProfileData;
    public GameResult GameResult;

    public Core(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content, int screenWidth, int screenHeight)
    {
      Ticks = 0;

      debugMessages = new List<Pair<string, int>>();
      debugWatches = new Dictionary<string, string>();
      debugWatchTtls = new Dictionary<string, int>();
      debugWatchesToRemove = new List<string>();

      GraphicsDevice = graphicsDevice;

      SpriteManager = new SpriteManager(this);
      SoundManager = new SoundManager(this);
      Renderer = new Renderer(this, spriteBatch, screenWidth, screenHeight);
      TimerManager = new TimerManager(this);

      MotionManager = new CMMotionManager();
      MotionManager.DeviceMotionUpdateInterval = 0.05;

      MessageManager = new MessageManager(this);
      MessageManager.Subscribe(MessageType.CoreEvent, this);

      Content = content;

      states = new Stack<State>();

      frameCounter = new FrameCounter();
      stopwatch = new Stopwatch();

      ProfileData = new ProfileData();

      Debug.Print(String.Format("screen size: {0}x{1}", screenWidth, screenHeight));
    }

    #region States

    private void ClearStates()
    {
      while (states.Count > 0)
      {
        var state = states.Pop();
        state.Leave();
        state.Unload();
      }

      states.Clear();
    }

    public void PushState(State state)
    {
      state.Load();
      state.Enter();
      states.Push(state);
    }

    public void PopState()
    {
      if (states.Count > 0)
      {
        var state = states.Pop();
        state.Leave();
        state.Unload();
      }
    }

    public State GetCurrentState()
    {
      return states.Peek();
    }

    #endregion

    public void Load()
    {
      Renderer.Load();
      SpriteManager.Load();
      SoundManager.Load();
      MessageManager.Load();

      Reset();
    }

    public void Unload()
    {
      ClearStates();

      MessageManager.Unload();
      SoundManager.Unload();
      SpriteManager.Unload();
      Renderer.Unload();
    }

    public void Update(GameTime gameTime)
    {
      TimerManager.Update(Ticks);

      // -------------------------------------

      if (Settings.DrawDebugMessages)
      {
        foreach (var message in debugMessages)
        {
          message.B--;
        }
        debugMessages.RemoveAll(m => m.B == 0);

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
        
      // -------------------------------------

      GetCurrentState().HandleInput();

      foreach (State state in states)
      {
        state.Update(Ticks);
      }

      MessageManager.Update(Ticks);
      Renderer.Update(Ticks);

      // -------------------------------------

      DebugWatch("fps", Math.Round(frameCounter.AverageFramesPerSecond).ToString());

      ++Ticks;
    }

    #region Debug
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
    #endregion

    public void Draw(GameTime gameTime)
    {
      var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
      frameCounter.Update(deltaTime);

      foreach (State state in states)
      {
        state.Draw();
      }

      if (Settings.DrawDebugMessages)
      {
        float i = -5;
        foreach (var message in debugMessages) {
          i += 10;
          Renderer["fg", 1000].DrawTextS(
            message.A,
            new Vector2(Renderer.ScreenWidth - 150, i),
            Color.White * 0.5f * ((float)message.B / 200)
          );
        }

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
        Renderer["fg", 999].DrawRectangleS(new Rectangle(0, 0, (int)maxWidth + 10, 7 * debugWatches.Count + 7), Color.Black * 0.5f);
      }

      // Final draw
      Renderer.Draw();
    }

    public void StartBenchmark()
    {
      stopwatch.Reset();
      stopwatch.Start();
    }

    public void StopBenchmark(int ticksTreshold = 10000)
    {
      stopwatch.Stop();

      if (debugMessages.Count < 5 && stopwatch.ElapsedTicks >= ticksTreshold)
      {
        DebugMessage(String.Format("Elapsed {0} ms, {1} ts", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
      }
    }

    // -------------------------------------------------

    private void TrackTilt(bool track)
    {
      // TODO: Fix or remove

//      if (track)
//      {
//        MotionManager.StartDeviceMotionUpdates(NSOperationQueue.CurrentQueue, (data, error) =>
//          {
//            // Math from 
//            // http://www.dulaccc.me/2013/03/computing-the-ios-device-tilt.html
//            // http://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Conversion
//
//            CMQuaternion q = data.Attitude.Quaternion;
//            this.deviceTilt = (float)Math.Atan(2 * (q.x * q.w + q.y * q.z)/(1 - 2 * (q.z * q.z + q.w * q.w)));
//          }
//        );
//      }
//      else
//      {
//        MotionManager.StopDeviceMotionUpdates();
//        deviceTilt = 0f;
//      }
    }

    // -------------------------------------------------

    private void Reset() 
    {
      ClearStates();
      PushState(new PlayState(this, ProfileData.CurrentArea));
      PushState(new MenuState(this));
      TrackTilt(true);
    }

    private void StartGame()
    {
      while (!(GetCurrentState() is PlayState || states.Count == 0)) {
        PopState();
      }

      var playState = GetCurrentState() as PlayState;
      if (playState != null)
      {
        playState.Start();
      }

      TrackTilt(false);
    }

    private void GameOver()
    {
      states.Push(new GameOverState(this));
      TrackTilt(true);
    }

    public void OnMessage(Message message, object sender)
    {
      switch (message.Type)
      {
        case MessageType.CoreEvent:
          switch ((message as CoreEventMessage).CoreEvent)
          {
            case CoreEvent.ResetGame:
              Reset();
              break;

            case CoreEvent.StartGame:
              StartGame();
              break;

            case CoreEvent.GameOver:
              GameOver();
              break;

          }
          break;
      }
    }
  }
}