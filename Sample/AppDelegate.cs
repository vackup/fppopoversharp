using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		//---- declarations
		UIWindow window;
		UINavigationController rootNavigationController;
		
		// This method is invoked when the application has loaded its UI and it is ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			this.window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			//---- instantiate a new navigation controller
			this.rootNavigationController = new UINavigationController ();
			this.rootNavigationController.PushViewController (new PopupDemoViewController (), false);
			
			//---- set the root view controller on the window. the nav controller will handle the rest
			this.window.RootViewController = this.rootNavigationController;
			
			this.window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

