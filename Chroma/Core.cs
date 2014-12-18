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
  sealed class Core
  {
    public SpriteManager SpriteManager { get; private set; }
    public Renderer Renderer { get; private set; }
    public ContentManager Content { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public MessageManager MessageManager { get; private set; }

    private readonly Stack<State> states;

    private int ticks = 0;
    private Animation animation;


    public Core(SpriteBatch spriteBatch, ContentManager content, int screenWidth, int screenHeight)
    {
      SpriteManager = new SpriteManager(this);
      SoundManager = new SoundManager(this);
      Renderer = new Renderer(this, spriteBatch, screenWidth, screenHeight);
      MessageManager = new MessageManager(this);
      Content = content;

      states = new Stack<State>();

      animation = new Animation();

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

      animation.Add("live", SpriteManager.GetAnimationFrames("projectile_red_", new List<int>{ 1, 2, 3, 4 }));
      animation.Play("live");

      ChangeStateTo(new PlayState(this));
    }

    public void Unload()
    {
      ClearStates();

      MessageManager.Unload();
      SoundManager.Unload();
      SpriteManager.Unload();
    }

    public void Update()
    {
      Renderer.Update(ticks);

      if (GetCurrentState() != null)
      {
        GetCurrentState().Update(ticks);
      }

      animation.Update(ticks);

      ++ticks;
    }

    public void Draw()
    {
      var glowRed = SpriteManager.GetSprite("glow_red");
      var projectile = SpriteManager.GetSprite("projectile_red_1");

      var v = new Vector2(10, 10);

      Renderer.Begin(BlendState.AlphaBlend);

      if (GetCurrentState() != null)
      {
        GetCurrentState().Draw();
      }

      Renderer.DrawSpriteS(glowRed, new Vector2(100, 25) - v, Color.White);
      Renderer.DrawSpriteS(animation.GetCurrentFrame(), new Vector2(100, 25), Color.White);

      Renderer.End();

      var blendState = BlendState.Additive;
      blendState.ColorSourceBlend = Blend.One;

      Renderer.Begin(blendState);
    
      Renderer.DrawSpriteS(glowRed, new Vector2(150, 25) - v, Color.White);

      Renderer.DrawSpriteS(animation.GetCurrentFrame(), new Vector2(150, 25), Color.White);

      Renderer.End();
    }
  }
}
