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

namespace Chroma
{
  public sealed class Core : ISubscriber
  {
    private readonly List<Pair<string, int>> debugMessages;

    public SpriteManager SpriteManager { get; private set; }
    public Renderer Renderer { get; private set; }
    public ContentManager Content { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public MessageManager MessageManager { get; private set; }
    public TimerManager TimerManager { get; private set; }

    private CMMotionManager MotionManager;
    public float deviceTilt { get; private set; }

    public GraphicsDevice GraphicsDevice;

    private readonly Stack<State> states;

    private FrameCounter frameCounter;
    public int Ticks { get; private set; }

    private readonly Stopwatch stopwatch;

    public ProfileData profileData;
    public GameResult gameResult;

    public Core(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content, int screenWidth, int screenHeight)
    {
      Ticks = 0;

      debugMessages = new List<Pair<string, int>>();
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

      profileData = new ProfileData();

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

      foreach (var message in debugMessages) 
      {
        message.B--;
      }
      debugMessages.RemoveAll(m => m.B == 0);

      // -------------------------------------

      GetCurrentState().HandleInput();

      foreach (State state in states)
      {
        state.Update(Ticks);
      }

      MessageManager.Update(Ticks);
      Renderer.Update(Ticks);

      // -------------------------------------

      ++Ticks;
    }

    public int GetTicks()
    {
      return Ticks;
    }

    public void DebugMessage(string message)
    {
      debugMessages.Insert(0, new Pair<string, int>(message, 200) );
    }

    public void Draw(GameTime gameTime)
    {
      var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
      frameCounter.Update(deltaTime);

      foreach (State state in states)
      {
        state.Draw();
      }

      if (Settings.DrawFps)
      {
        Renderer.DrawTextS(
          String.Format("{0} fps", Math.Round(frameCounter.AverageFramesPerSecond)),
          new Vector2(10, 5),
          Color.White * 0.25f
        );
      }

      if (Settings.DrawDebugMessages)
      {
        float i = -5;
        foreach (var message in debugMessages) {
          i += 10;
          Renderer.DrawTextS(
            message.A,
            new Vector2(Renderer.ScreenWidth - 150, i),
            Color.White * 0.5f * ((float)message.B / 200)
          );
        }
      }

      // Final draw
      Renderer.Draw();
    }

    public void StartBenchmark()
    {
      stopwatch.Reset();
      stopwatch.Start();
    }

    public void StopBenchmark(bool printOnlyPositiveMs = false)
    {
      stopwatch.Stop();

      if (debugMessages.Count < 10 && (!printOnlyPositiveMs || (printOnlyPositiveMs && stopwatch.ElapsedMilliseconds > 0)))
      {
        DebugMessage(String.Format("Elapsed {0} ms, {1} ts", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
      }
    }

    // -------------------------------------------------

    private void TrackTilt(bool track)
    {
      if (track)
      {
        MotionManager.StartDeviceMotionUpdates(NSOperationQueue.CurrentQueue, (data, error) =>
          {
            // Math from 
            // http://www.dulaccc.me/2013/03/computing-the-ios-device-tilt.html
            // http://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Conversion

            CMQuaternion q = data.Attitude.Quaternion;
            this.deviceTilt = (float)Math.Atan(2 * (q.x * q.w + q.y * q.z)/(1 - 2 * (q.z * q.z + q.w * q.w)));
          }
        );
      }
      else
      {
        MotionManager.StopDeviceMotionUpdates();
        deviceTilt = 0f;
      }
    }

    // -------------------------------------------------

    private void Reset() 
    {
      ClearStates();
      PushState(new PlayState(this, profileData.CurrentArea));
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
          switch ((message as CoreEventMessage).coreEvent)
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