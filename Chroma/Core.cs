using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Chroma.Actors;
using Chroma.Messages;
using Chroma.States;
using Chroma.Graphics;

namespace Chroma
{
  public sealed class Core
  {
    public SpriteManager SpriteManager { get; private set; }
    public Renderer Renderer { get; private set; }
    public ContentManager Content { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public MessageManager MessageManager { get; private set; }
    public TimerManager TimerManager { get; private set; }

    private readonly Stack<State> states;
    private readonly Random random;

    private FrameCounter frameCounter;
    private int ticks = 0;

    public Core(SpriteBatch spriteBatch, ContentManager content, int screenWidth, int screenHeight)
    {
      SpriteManager = new SpriteManager(this);
      SoundManager = new SoundManager(this);
      Renderer = new Renderer(this, spriteBatch, screenWidth, screenHeight);
      MessageManager = new MessageManager(this);
      TimerManager = new TimerManager(this);

      Content = content;
      random = new Random();

      states = new Stack<State>();

      frameCounter = new FrameCounter();

      Debug.Print(String.Format("Screen size: {0}x{1}", screenWidth, screenHeight));
    }

    public int GetRandom(int from, int to)
    {
      return (random.Next() % (to - from + 1)) + from;
    }

    public bool ChanceRoll(float chance = 0.5f)
    {
      return GetRandom(1, 100) <= chance * 100;
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

      if (GetCurrentState() != null)
      {
        GetCurrentState().Update(ticks);
      }

      MessageManager.Update(ticks);

      Renderer.Update(ticks);

      ++ticks;
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
          String.Format("fps:    {0}", Math.Round(frameCounter.AverageFramesPerSecond)), 
          new Vector2(Renderer.ScreenWidth - 70, 3),
          Color.White * 0.25f
        );
      }

      // final draw
      Renderer.Draw();
    }
  }
}