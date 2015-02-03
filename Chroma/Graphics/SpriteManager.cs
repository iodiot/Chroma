using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Chroma.Graphics
{
  public sealed class Sprite
  {
    public string TextureName;
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public int LinkX;
    public int LinkY;
    public Vector2 AnchorPoint;

    public Sprite ClampWidth(int maxWidth)
    {
      var result = this.MemberwiseClone() as Sprite;
      result.Width = Math.Min(Width, maxWidth);
      return result;
    }

    public Sprite Reduce(int left, int top, int right, int bottom)
    {
      var result = this.MemberwiseClone() as Sprite;
      result.X += Math.Min(left, Width);
      result.Y += Math.Min(top, Height);
      result.Width = Math.Max(Width - left - right, 0);
      result.Height = Math.Max(Height - top - bottom, 0);
      return result;
    }
  }

  public sealed class SpriteManager
  {
    // shortcuts
    public Texture2D OnePixel { get; private set; }
    public Texture2D Font { get; private set; }
    public Sprite OnePixelSprite { get; private set; }

    private readonly Core core;
    private readonly Dictionary<string, Sprite> sprites;
    private readonly Dictionary<string, Texture2D> textures;
    private readonly Dictionary<string, Color[]> texturesData;

    public SpriteManager(Core core)
    {
      this.core = core;

      sprites = new Dictionary<string, Sprite>();
      textures = new Dictionary<string, Texture2D>();
      texturesData = new Dictionary<string, Color[]>();
    }

    public void Load()
    {
      LoadFromFile("atlas.json");

      OnePixel = core.Content.Load<Texture2D>(@"Images/one.png");
      Font = core.Content.Load<Texture2D>(@"Images/font.png");

      AddTexture("one", OnePixel);
      AddTexture("font", Font);

      OnePixelSprite = new Sprite() { Width = 1, Height = 1, TextureName = "one" };
    }

    public void Unload()
    {

    }

    private void AddTexture(string textureName, Texture2D texture)
    {
      textures.Add(textureName, texture);

      var colors = new Color[texture.Width * texture.Height];
      texture.GetData<Color>(colors);
      texturesData.Add(textureName, colors);
    }

    private void LoadFromFile(string fileName)
	  {
      JsonValue results;
      using (var streamReader = new StreamReader(String.Format(@"Content/Images/{0}", fileName)))
      {
        results = JsonObject.Load(streamReader);
      }

      string textureName = results["image-path"].ToString().Replace("\"", "");

      AddTexture(textureName, core.Content.Load<Texture2D>(String.Format(@"Images/{0}", textureName)));

      var array = (JsonArray)results["sprites"];
      foreach (var sprite in array)
      {
        var name = sprite["name"];
        var x = sprite["x"];
        var y = sprite["y"];
        var width = sprite["width"];
        var height = sprite["height"];
        var linkX = 0;
        var linkY = 0;
        if (sprite["link"] == "true")
        {
          linkX = sprite["link-x"];
          linkY = sprite["link-y"];
        }

        sprites.Add(
          name,
          new Sprite() { X = x, Y = y, Width = width, Height = height, TextureName = textureName, LinkX = linkX, LinkY = linkY }
        );
      }
    }

    public Sprite GetSprite(string name)
    {
      Debug.Assert(sprites.ContainsKey(name), String.Format("SpriteManager.GetSprite() : Sprite {0} is missing", name));

      var s = sprites[name];
      return new Sprite() { 
        X = s.X, Y = s.Y, 
        Width = s.Width, Height = s.Height, 
        TextureName = s.TextureName, 
        LinkX = s.LinkX, LinkY = s.LinkY 
      };
    }

    public Texture2D GetTexture(string name)
    {
      Debug.Assert(textures.ContainsKey(name), String.Format("SpriteManager.GetTexture() : Texture {0} is missing", name));

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

    public Color[] GetTextureData(string name)
    {
      Debug.Assert(texturesData.ContainsKey(name), String.Format("SpriteManager.GetTextureData() : Texture {0} is missing", name));

      return texturesData[name];
    }

    public Sprite GetFontSprite(char ch)
    {
      const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ.,!?\"'/\\<>()[]{}abcdefghijklmnopqrstuvwxyz_               0123456789+-=*:;                          ";
      const int CharsPerRow = 42;
      const int CharWidth = 6;
      const int CharHeight = 8;

      var n = Chars.IndexOf(ch);

      var sprite = new Sprite() {
        X = (n % CharsPerRow) * CharWidth,
        Y = (n / CharsPerRow) * CharHeight,
        Width = CharWidth,
        Height = CharHeight,
        TextureName = "font"
      };

      return sprite;
    }
  }
}
