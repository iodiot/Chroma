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

    public GameOverState(Core core) : base(core)
    {
    }

    public override void Update(int ticks)
    {
      if (wasTouched)
      {
        core.MessageManager.Send(new CoreEventMessage(CoreEvent.ResetGame), this);
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.FillScreen(Color.Black * 0.5f);

      core.Renderer.DrawTextS("Distance: " + core.gameResult.distance.ToString(), new Vector2(45, 45), Color.White, 4);

      base.Draw();
    }

    public override void HandleInput()
    {
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

