using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Graphics;
using Chroma.Messages;

namespace Chroma.States
{
  public class MenuState : State
  {
    private static readonly string Title = "CHROMA";

    private bool wasTouched = false;

    public MenuState(Core core) : base(core)
    {
    }

    public override void Update(int ticks)
    {
      if (wasTouched)
      {
        core.MessageManager.Send(new CoreEventMessage(CoreEvent.StartGame), this);
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer[1000].FillScreen(Color.Black * 0.1f);

      core.Renderer[1000].DrawTextS(Title, new Vector2(45, 45), Color.White, 4);

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
