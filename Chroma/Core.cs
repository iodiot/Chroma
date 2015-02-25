using System;
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

namespace Chroma
{
  public sealed class Core
  {
    private readonly List<Pair<string, int>> DebugMessages;

    public SpriteManager SpriteManager { get; private set; }
    public Renderer Renderer { get; private set; }
    public ContentManager Content { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public MessageManager MessageManager { get; private set; }
    public TimerManager TimerManager { get; private set; }

    public GraphicsDevice GraphicsDevice;

    private readonly Stack<State> states;

    private FrameCounter frameCounter;
    private int ticks = 0;

    public Core(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content, int screenWidth, int screenHeight)
    {
      DebugMessages = new List<Pair<string, int>>();
      GraphicsDevice = graphicsDevice;

      SpriteManager = new SpriteManager(this);
      SoundManager = new SoundManager(this);
      Renderer = new Renderer(this, spriteBatch, screenWidth, screenHeight);
      MessageManager = new MessageManager(this);
      TimerManager = new TimerManager(this);

      Content = content;

      states = new Stack<State>();

      frameCounter = new FrameCounter();

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

    public void ChangeStateTo(State state)
    {
      ClearStates();

      state.Load();
      state.Enter();
      states.Push(state);
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

      ChangeStateTo(new PlayState(this));
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
      TimerManager.Update(ticks);

      foreach (var message in DebugMessages) {
        message.B--;
      }
      DebugMessages.RemoveAll(m => m.B == 0);

      if (GetCurrentState() != null)
      {
        GetCurrentState().Update(ticks);
      }

      MessageManager.Update(ticks);

      Renderer.Update(ticks);

      ++ticks;
    }

    public int GetTicks()
    {
      return ticks;
    }

    public void DebugMessage(string message)
    {
      DebugMessages.Insert(0, new Pair<string, int>(message, 200) );
    }

    public void Draw(GameTime gameTime)
    {
      var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
      frameCounter.Update(deltaTime);

      if (GetCurrentState() != null)
      {
        GetCurrentState().Draw();
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
        foreach (var message in DebugMessages) {
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
  }
}