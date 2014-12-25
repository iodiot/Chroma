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

    private readonly Stack<State> states;

    private FrameCounter frameCounter;
    private int ticks = 0;

    public Core(SpriteBatch spriteBatch, ContentManager content, int screenWidth, int screenHeight)
    {
      SpriteManager = new SpriteManager(this);
      SoundManager = new SoundManager(this);
      Renderer = new Renderer(this, spriteBatch, screenWidth, screenHeight);
      MessageManager = new MessageManager(this);
      Content = content;

      states = new Stack<State>();

      frameCounter = new FrameCounter();

      Log.Print(String.Format("Screen size: {0}x{1}", screenWidth, screenHeight));
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
    }

    public void Update(GameTime gameTime)
    {
      Renderer.Update(ticks);

      if (GetCurrentState() != null)
      {
        GetCurrentState().Update(ticks);
      }

      MessageManager.Update(ticks);

      ++ticks;
    }

    public void Draw(GameTime gameTime)
    {
      var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
      frameCounter.Update(deltaTime);

      Renderer.Begin(BlendState.AlphaBlend);

      if (GetCurrentState() != null)
      {
        GetCurrentState().Draw();
      }

      Renderer.DrawTextS(
        String.Format("fps:{0}", Math.Round(frameCounter.AverageFramesPerSecond)), 
        new Vector2(Renderer.ScreenWidth - 40, 3),
        Color.White * 0.25f
      );

      Renderer.End();
    }
  }
}
