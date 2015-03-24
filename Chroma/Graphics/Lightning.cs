using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Chroma.Helpers;

namespace Chroma.Graphics
{
  class LightningVertex
  {
    public Vector2 Position;
  }

  public sealed class Lightning
  {
    private readonly Core core;
    private readonly List<LightningVertex> vertices;

    public Lightning(Core core, Vector2 from, Vector2 to, int maxRecursiveDepth = 3, float maxDeviation = 25f)
    {
      this.core = core;

      vertices = new List<LightningVertex>();
      vertices.Add(new LightningVertex() { Position = from });
      GenerateVertices(from, to, vertices, maxDeviation, 0, maxRecursiveDepth);      
      vertices.Add(new LightningVertex() { Position = to });
    }

    private static void GenerateVertices(Vector2 from, Vector2 to, List<LightningVertex> vertices, float maxDeviation, int currentDepth, int maxDepth)
    {
      if (currentDepth == maxDepth)
      {
        return;
      }

      const float D = .1f;

      var length = (to - from).Length();
      var r = SciHelper.GetRandom(D, 1f - D);
      var dir = to - from;
      dir.Normalize();

      var perp = new Vector2(1f, -dir.X / (dir.Y + .01f));
      perp.Normalize();

      var currentDeviation = (float)(maxDepth - currentDepth) / (float)maxDepth * maxDeviation * SciHelper.GetRandom(-1f, 1f);

      var randomPosition = from + r * length * dir + perp * currentDeviation;

      GenerateVertices(from, randomPosition, vertices, maxDeviation, currentDepth + 1, maxDepth);
      vertices.Add(new LightningVertex() { Position = randomPosition });
      GenerateVertices(randomPosition, to, vertices, maxDeviation, currentDepth + 1, maxDepth);
    }


    public void Draw(Color color)
    {
      /*for (var i = 1; i < vertices.Count; ++i)
      {
        core.Renderer["add"].DrawLineW(vertices[i - 1].Center, vertices[i].Center, color * .25f, 3f);
      }*/

      for (var i = 1; i < vertices.Count; ++i)
      {
        core.Renderer["add"].DrawLineW(vertices[i - 1].Position, vertices[i].Position, color * .75f);
      }

      /*for (var i = 1; i < vertices.Count - 1; ++i)
      {
        core.Renderer["add"].DrawSpriteW(core.SpriteManager.GetSprite(SpriteName.glow), vertices[i].Center - new Vector2(2.5f, 2.5f),
          scale: new Vector2(.1f, .1f), tint: color * .75f);
      }*/
    }
  }
}

