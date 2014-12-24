using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Graphics;
using Chroma.Particles;

namespace Chroma.States
{
  public class MenuState : State
  {
    private static readonly string Title = "CHROMA";
    private static readonly float TitleScale = 4.0f;
    private static readonly Vector2 Indent = new Vector2(45, 45);

    private readonly ParticleManager particleManager; 

    private bool wasTouched = false;

    public MenuState(Core core) : base(core)
    {
     // particleManager = new ParticleManager(
     //   core, 
      //);

      for (var i = 0; i < Title.Length; ++i)
      {
        particleManager.AddParticlesFromSprite(
          core.SpriteManager.GetFontSprite(Title[i]), 
          new Vector2(Indent.X + i * TitleScale * 6, Indent.Y),
          Vector2.Zero,
          TitleScale
        );
      }
    }


    public override void Update(int ticks)
    {
      HandleInput();

      if (wasTouched)
      {
        particleManager.Update(ticks);
      }

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.FillScreen(Color.Black);

      if (wasTouched)
      {
        particleManager.Draw(TitleScale);
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
