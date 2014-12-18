using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Chroma
{
  class Game1 : Game
  {
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Core core;

    public Game1()
      : base()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
      graphics.PreferredBackBufferWidth /= Settings.ScaleFactor;
      graphics.PreferredBackBufferHeight /= Settings.ScaleFactor;
      graphics.ApplyChanges();

      base.Initialize();
    }

    protected override void LoadContent()
    {
      spriteBatch = new SpriteBatch(GraphicsDevice);

      core = new Core(spriteBatch, Content, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
      core.Load();
    }

    protected override void UnloadContent()
    {
      core.Unload();
    }

    protected override void Update(GameTime gameTime)
    {
      core.Update();

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(new Color(54, 74, 74));

      core.Draw();

      base.Draw(gameTime);
    }
  }
}
