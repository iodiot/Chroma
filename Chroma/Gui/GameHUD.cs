﻿using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.States;
using Chroma.Actors;
using Chroma.Gameplay;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;

namespace Chroma.Gui
{

  public class Button 
  {
    public MagicColor color;
    public Rectangle box;
    public bool pressed;
  }

  public class ButtonSwipe
  {
    public Button First;
    public Button Second;
  }
    
  public class GameHUD : Gui
  {
    private PlayState playState;
    private PlayerActor player;

    private Dictionary<MagicColor, Button> buttons;
    private Dictionary<int, ButtonSwipe> swipes;
    private int jumpTouch = -1;

    public GameHUD(Core core, PlayState playState, PlayerActor player) : base(core)
    {
      this.playState = playState;
      this.player = player;

      #region Creating buttons
      int bw = 55;
      int bp = 0;
      buttons = new Dictionary<MagicColor, Button>();
      buttons.Add(MagicColor.Red, new Button() { 
        color = MagicColor.Red, 
        box = new Rectangle((int)core.Renderer.ScreenWidth - (bw + bp) * 2, (int)core.Renderer.ScreenHeight - (bw + bp) * 2, bw, bw),
        pressed = false
      });
      buttons.Add(MagicColor.Yellow, new Button() { 
        color = MagicColor.Yellow, 
        box = new Rectangle((int)core.Renderer.ScreenWidth - (bw + bp), (int)core.Renderer.ScreenHeight - (bw + bp) * 2, bw, bw),
        pressed = false
      });
      buttons.Add(MagicColor.Blue, new Button() { 
        color = MagicColor.Blue, 
        box = new Rectangle((int)core.Renderer.ScreenWidth - (int)((bw + bp) * 1.5), (int)core.Renderer.ScreenHeight - (bw + bp), bw, bw),
        pressed = false
      });
      #endregion

      swipes = new Dictionary<int, ButtonSwipe>();
    }

    public override void Update(int ticks)
    {
      HandleInput();
      base.Update(ticks);
    }

    public override void Draw()
    {
      // Jump button
      core.Renderer.DrawSpriteS(core.SpriteManager.GetSprite("btn_jump" + (jumpTouch == -1 ? "" : "_pressed")), 
        new Vector2(20, core.Renderer.ScreenHeight - 100), Color.White, 1.5f);

      // Tripad
      core.Renderer.DrawSpriteS(core.SpriteManager.GetSprite("pad_base"), 
        new Vector2(buttons[MagicColor.Red].box.Left, buttons[MagicColor.Red].box.Top) - new Vector2(7, 5), Color.White, 1.5f);
      if (player.charging)
        core.Renderer["fg_add"].DrawSpriteS(core.SpriteManager.GetSprite("glow"), 
          new Vector2(buttons[MagicColor.Red].box.Left, buttons[MagicColor.Red].box.Top) - new Vector2(13, 13),
          MagicManager.MagicColors[player.chargeColor] * 0.7f, 2.0f);
      foreach (KeyValuePair<MagicColor, Button> pair in buttons)
      {
        Button button = pair.Value;
        string gemName = button.color == MagicColor.Red ? "red" : button.color == MagicColor.Yellow ? "yellow" : "blue";
        gemName = "gem_" + gemName + (button.pressed ? "_active" : "");
        if (button.pressed)
          core.Renderer["fg_add"].DrawSpriteS(core.SpriteManager.GetSprite("glow"), new Vector2(button.box.Left, button.box.Top) - new Vector2(25, 25),
            MagicManager.MagicColors[button.color] * 0.8f, 1.5f);
        core.Renderer.DrawSpriteS(core.SpriteManager.GetSprite(gemName), new Vector2(button.box.Left, button.box.Top), Color.White, 1.5f);
      }

      base.Draw();
    }

    public void HandleInput()
    {
      var touchState = TouchPanel.GetState();

      foreach (TouchLocation touch in touchState)
      {

        // Jump
        if (touch.Id == jumpTouch && touch.State == TouchLocationState.Released)
        {
          jumpTouch = -1;
          continue;
        }
        if (touch.Position.X < core.Renderer.ScreenWidth / 2)
        {
          switch (touch.State) 
          {
            case TouchLocationState.Pressed:
              jumpTouch = touch.Id;
              player.TryToJump();
              break;
          }
          continue;
        }

        bool overButton = false;
        foreach (KeyValuePair<MagicColor, Button> pair in buttons)
        {
          Button b = pair.Value;
          if (b.box.Contains(new Point((int)touch.Position.X, (int)touch.Position.Y)))
          {
            overButton = true;
            ButtonSwipe swipe;
            switch (touch.State)
            {
              case TouchLocationState.Pressed:
                swipe = new ButtonSwipe() { First = b, Second = null };
                swipes.Add(touch.Id, swipe);
                Charge(swipe);
                break;
              case TouchLocationState.Moved:
                if (swipes.TryGetValue(touch.Id, out swipe) && (swipe.Second != b))
                {
                  if (swipe.Second != null)
                    swipe.Second.pressed = false;
                  if (b != swipe.First)
                    swipe.Second = b;
                  else
                    swipe.Second = null;
                  Charge(swipe);
                }
                break;
              case TouchLocationState.Released:
                if (swipes.TryGetValue(touch.Id, out swipe))
                {
                  Shoot();
                  swipes.Remove(touch.Id);
                }
                break;
            }
          }
        }

        // Rleased over nothing
        if (!overButton && touch.State == TouchLocationState.Released)
        {
          ButtonSwipe swipe;
          if (swipes.TryGetValue(touch.Id, out swipe))
          {
            Shoot();
            swipes.Remove(touch.Id);
          }
        }

      }
    }

    private void Charge(ButtonSwipe swipe) 
    {
      swipe.First.pressed = true;
      if (swipe.Second != null)
      {
        swipe.Second.pressed = true;
      }
        
      player.Charge(swipe.First.color, swipe.Second == null ? 0 : swipe.Second.color);

    }
  
    private void Shoot() 
    {
      buttons[MagicColor.Red].pressed = buttons[MagicColor.Yellow].pressed = buttons[MagicColor.Blue].pressed = false;
      player.Shoot();
    }

  }
}