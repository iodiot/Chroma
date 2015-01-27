using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Graphics;

namespace Chroma.States
{
  public class MenuState : State
  {
    private static readonly string Title = "CHROMA";
    private static readonly float TitleScale = 4.0f;
    private static readonly Vector2 Indent = new Vector2(45, 45);

    private bool wasTouched = false;

    public MenuState(Core core) : base(core)
    {
    }


    public override void Update(int ticks)
    {
      HandleInput();

      if (wasTouched)
      {
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.FillScreen(Color.Black);

      if (wasTouched)
      {
      }
      else
      {
        core.Renderer.DrawTextS(Title, new Vector2(45, 45), Color.White, 4);
      }
      base.Draw();
    }

    private void HandleInput()
    {
      var touchState = TouchPanel.GetState();

      if (touchState.Count == 1 && !wasTouched)
      {
        wasTouched = true;
      }
    }
  }
}
