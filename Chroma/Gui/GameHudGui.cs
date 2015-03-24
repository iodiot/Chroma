using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.States;
using Chroma.Actors;
using Chroma.Gameplay;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

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
    
  public class GameHudGui : Gui
  {
    // DEBUG
    private bool stop = false;

    private PlayState playState;
    private PlayerActor player;

    private Dictionary<MagicColor, Button> buttons;
    private Dictionary<int, ButtonSwipe> swipes;

    private Rectangle jumpBox;
    private int jumpTouchId = -1;

    public GameHudGui(Core core, PlayState playState, PlayerActor player) : base(core)
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

      jumpBox = new Rectangle(
        0, (int)core.Renderer.ScreenHeight - 110,
        110, 110
      );
      #endregion

      swipes = new Dictionary<int, ButtonSwipe>();
    }

    public override void Update(int ticks)
    {
      // DEBUG
      if (stop)
        player.Position.X -= 2.0f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      // Jump button
      core.Renderer["fg"].DrawSpriteS(core.SpriteManager.GetSprite("btn_jump" + (jumpTouchId == -1 ? "" : "_pressed")), 
        new Vector2(20, core.Renderer.ScreenHeight - 100), scale: new Vector2(1.5f, 1.5f));

      // Tripad
      core.Renderer["fg"].DrawSpriteS(core.SpriteManager.GetSprite("pad_base"), 
        new Vector2(buttons[MagicColor.Red].box.Left, buttons[MagicColor.Red].box.Top) - new Vector2(7, 5), scale: new Vector2(1.5f, 1.5f));

      if (player.charging)
        core.Renderer["fg_add"].DrawGlowS(
          new Vector2(buttons[MagicColor.Red].box.Left, buttons[MagicColor.Red].box.Top) - new Vector2(0, 13),
          MagicManager.MagicColors[player.chargeColor] * 0.7f, 
          50, false
        );

      foreach (KeyValuePair<MagicColor, Button> pair in buttons)
      {
        Button button = pair.Value;
        string gemName = button.color == MagicColor.Red ? "red" : button.color == MagicColor.Yellow ? "yellow" : "blue";
        gemName = "gem_" + gemName + (button.pressed ? "_active" : "");
        if (button.pressed)
          core.Renderer["fg_add"].DrawGlowS(
            new Vector2(button.box.Left + 21, button.box.Top + 21),
            MagicManager.MagicColors[button.color] * 0.8f,
            40
          );

        core.Renderer["fg"].DrawSpriteS(core.SpriteManager.GetSprite(gemName), new Vector2(button.box.Left, button.box.Top), scale: new Vector2(1.5f, 1.5f));
      }

      base.Draw();
    }

    public void HandleInput()
    {
    
      foreach (TouchLocation touch in core.TouchState)
      {

        // Debug: stop
        if (touch.Position.Y < core.Renderer.ScreenHeight / 3 && touch.Position.X > core.Renderer.ScreenWidth / 2 
          && touch.State == TouchLocationState.Released)
        {
          stop = !stop;
          core.DebugMessage(stop ? "[Stopped]" : "[Resumed]");
        }

        // Debug: show colliders
        if (touch.Position.Y < core.Renderer.ScreenHeight / 3 && touch.Position.X < core.Renderer.ScreenWidth / 2 
          && touch.State == TouchLocationState.Released)
        {
          Settings.DrawBoundingBoxes = !Settings.DrawBoundingBoxes;
          Settings.DrawColliders = !Settings.DrawColliders;

          core.DebugMessage(Settings.DrawBoundingBoxes ? "[Rectangles shown]" : "[Rectangles hidden]");
        }

        // Jump
        if (touch.Id == jumpTouchId && touch.State == TouchLocationState.Released)
        {
          jumpTouchId = -1;
          player.StopJump();
          continue;
        }
        if (!swipes.ContainsKey(touch.Id) && jumpBox.Contains((int)touch.Position.X, (int)touch.Position.Y))
        {
          switch (touch.State) 
          {
            case TouchLocationState.Pressed:
              jumpTouchId = touch.Id;
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

        // Released over nothing
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