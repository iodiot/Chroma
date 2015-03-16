using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Actors;
using Chroma.Messages;
using Chroma.Graphics;
using Chroma.Gui;
using Chroma.Gameplay;

namespace Chroma.States
{
  public class GameOverState : State
  {
    private bool wasTouched = false;
    private int timeout = 50;
    private int ticks;

    public GameOverState(Core core) : base(core)
    {
    }

    public override void Update(int ticks)
    {
      this.ticks = ticks;

      if (timeout > 0)
        timeout--;

      if (wasTouched)
      {
        core.MessageManager.Send(new CoreEventMessage(CoreEvent.ResetGame), this);
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer["fg"].FillScreen(Color.Black * 0.5f);

      core.Renderer["fg"].DrawTextS("Distance: " + core.gameResult.distance.ToString() + " m", new Vector2(45, 60), Color.White, 3);

      if (timeout == 0)
        core.Renderer["fg"].DrawTextS("Tap to retry", new Vector2(45, 100), Color.White * ((1 + (float)Math.Sin((float)ticks / 10)) / 2f), 2);

      base.Draw();
    }

    public override void HandleInput()
    {
      if (timeout > 0)
        return;

      var touchState = TouchPanel.GetState();

      foreach (var touch in touchState)
      {
        if (touch.State == TouchLocationState.Released)
        {
          wasTouched = true;
        }
      }
    }
  }
}

