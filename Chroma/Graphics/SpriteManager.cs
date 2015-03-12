using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Chroma.Gameplay;

namespace Chroma.Graphics
{
  public sealed class Sprite
  {
    public int X;
    public int Y;
    public int SrcWidth;
    public int SrcHeight;
    public int Width;
    public int Height;
    public int OffX;
    public int OffY;
    public int LinkX;
    public int LinkY;
    public string TextureName;

    public Sprite ClampWidth(int maxWidth)
    {
      var result = Clone();
      result.SrcWidth = Math.Min(SrcWidth, maxWidth);
      return result;
    }

    public Sprite Reduce(int left, int top, int right, int bottom)
    {
      var result = Clone();
      result.X += Math.Min(left, SrcWidth);
      result.Y += Math.Min(top, SrcHeight);
      result.SrcWidth = Math.Max(SrcWidth - left - right, 0);
      result.SrcHeight = Math.Max(SrcHeight - top - bottom, 0);
      return result;
    }

    public Sprite Clone()
    {
      return this.MemberwiseClone() as Sprite;
    }

    public Vector2 GetLink()
    {
      return new Vector2(LinkX, LinkY);
    }

    public Vector2 GetOffset()
    {
      return new Vector2(OffX, OffY);
    }
  }

  public sealed class SpriteManager
  {
    // Shortcuts
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

      OnePixelSprite = new Sprite() { SrcWidth = 1, SrcHeight = 1, TextureName = "one" };
    }

    public void Unload()
    {
      sprites.Clear();
      textures.Clear();
      texturesData.Clear();
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

        var fullWidth = width;
        var fullHeight = height;
        if (sprite.ContainsKey("full-width"))
        {
          fullWidth = sprite["full-width"];
          fullHeight = sprite["full-height"];
        }

        var offX = 0;
        var offY = 0;
        if (sprite.ContainsKey("off-x"))
        {
          offX = sprite["off-x"];
          offY = sprite["off-y"];
        }

        var linkX = 0;
        var linkY = 0;
        if (sprite["link"] == "true")
        {
          linkX = sprite["link-x"];
          linkY = sprite["link-y"];
        }

        sprites.Add(
          name,
          new Sprite() {  
            TextureName = textureName, 
            X = x, Y = y, 
            SrcWidth = width, SrcHeight = height,
            Width = fullWidth, Height = fullHeight,
            OffX = offX, OffY = offY,
            LinkX = linkX, LinkY = linkY 
          }
        );
      }
    }

    public Sprite GetSprite(string name, MagicColor? color = null)
    {
      if (color != null)
      {
        name += "/" + color.ToString();
      }

      Debug.Assert(sprites.ContainsKey(name), String.Format("SpriteManager.GetSprite() : Sprite {0} is missing", name));

      // TODO
      return sprites[name].Clone();
    }

    public Sprite GetSprite(SpriteName name, MagicColor? color = null)
    {
      return GetSprite(name.ToString(), color);
    }

    public Texture2D GetTexture(string name)
    {
      Debug.Assert(textures.ContainsKey(name), String.Format("SpriteManager.GetTexture() : Texture {0} is missing", name));

      return textures[name];
    }

    public List<Sprite> GetFrames(string prefix, IEnumerable<int> frames, MagicColor? color = null)
    {
      var result = new List<Sprite>();

      foreach (var frame in frames) 
      {
        result.Add(GetSprite(String.Format("{0}{1}", prefix, frame), color));
      }

      return result;
    }

    public Color[] GetTextureData(string name)
    {
      Debug.Assert(texturesData.ContainsKey(name), String.Format("SpriteManager.GetTextureData() : Texture {0} is missing", name));

      return texturesData[name];
    }

    public Sprite MakeCharSprite(char ch)
    {
      const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ.,!?\"'/\\<>()[]{}abcdefghijklmnopqrstuvwxyz_               0123456789+-=*:;                          ";
      const int CharsPerRow = 42;
      const int CharWidth = 6;
      const int CharHeight = 8;

      var n = Chars.IndexOf(ch);

      var sprite = new Sprite() {
        X = (n % CharsPerRow) * CharWidth,
        Y = (n / CharsPerRow) * CharHeight,
        SrcWidth = CharWidth,
        SrcHeight = CharHeight,
        TextureName = "font"
      };

      return sprite;
    }
  }
}
