using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Json;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Chroma.Graphics
{
  sealed class SpriteManager
  {
    public Texture2D OnePixel { get; private set; }

    private readonly Core core;
    private readonly Dictionary<string, Sprite> sprites;
    private readonly Dictionary<string, Texture2D> textures;

    public SpriteManager(Core core)
    {
      this.core = core;

      sprites = new Dictionary<string, Sprite>();
      textures = new Dictionary<string, Texture2D>();
    }

    public void Load()
    {
      LoadFromFile("atlas.json");

      OnePixel = core.Content.Load<Texture2D>(@"Images/one.png");

      textures.Add("one", OnePixel);
    }

    public void Unload()
    {

    }

    private void LoadFromFile(string fileName)
	  {
      JsonValue results;
      using (var streamReader = new StreamReader(String.Format(@"Content/Images/{0}", fileName)))
      {
        results = JsonObject.Load(streamReader);
      }

      string textureFileName = results["image-path"].ToString().Replace("\"", "");

      // load texture
      textures.Add(textureFileName, core.Content.Load<Texture2D>(String.Format(@"Images/{0}", textureFileName)));

      var array = (JsonArray)results["sprites"];
      foreach (var sprite in array)
      {
        var name = sprite["name"];
        var x = sprite["x"];
        var y = sprite["y"];
        var width = sprite["width"];
        var height = sprite["height"];

        sprites.Add(name, new Sprite(x, y, width, height, textureFileName));
      }
    }

    public Sprite GetSprite(string name)
    {
      Debug.Assert(sprites.ContainsKey(name), String.Format("SpriteManager.GetSprite() - Sprite {0} is missing", name));

      return sprites[name];
    }

    public Texture2D GetTexture(string name)
    {
      Debug.Assert(textures.ContainsKey(name), String.Format("SpriteManager.GetTexture() - Texture {0} is missing", name));

      return textures[name];
    }

    public List<Sprite> GetFrames(string prefix, IEnumerable<int> frames)
    {
      var result = new List<Sprite>();

      foreach (var frame in frames) 
      {
        result.Add(GetSprite(String.Format("{0}{1}", prefix, frame)));
      }

      return result;
    }
  }
}
