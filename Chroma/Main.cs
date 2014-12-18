using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using Chroma;

namespace Application
{
  [Register ("AppDelegate")]
  class Program : UIApplicationDelegate 
  {
    Game1 game;
    public override void FinishedLaunching (UIApplication app)
    {
      // Fun begins..
      game = new Game1 ();
      game.Run ();
    }

    static void Main (string [] args)
    {
      UIApplication.Main(args, null, "AppDelegate");
    }
  }
}
