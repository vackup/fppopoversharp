using System;
using MonoTouch.UIKit;
using System.Drawing;
using FPPopoverSharp;

namespace Sample
{
	public class PopupDemoViewController : UIViewController
	{
		UIButton _rootButton;
		DemoTableController _homController;

		void SetTestButton ()
		{
			_rootButton = UIButton.FromType (UIButtonType.RoundedRect);
			_rootButton.Frame = new RectangleF (75, 300, 200, 50);
			_rootButton.SetTitle ("Click Me!", UIControlState.Normal);
			_rootButton.TouchUpInside += (sender, e) => {

				var button = sender as UIButton;

				var popover = new FPPopoverController (_homController);
				popover.PresentPopoverFromView (button);
			};
			this.View.AddSubview (_rootButton);
		}

		RectangleF GetPopOverRectangle (RectangleF frame, bool isFullScreen, bool isInToolBar)
		{
			var statusBarHeight = 0;

			if (isFullScreen) {
				statusBarHeight = 40;
			}

			var toolBarHeight = 0;

			if (isInToolBar) {
				toolBarHeight = (int)View.Bounds.Height - 44;
			}

			return new RectangleF (frame.X, frame.Y + statusBarHeight + toolBarHeight, frame.Width, frame.Height);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.White;

			NavigationItem.Title = "Adding a Toolbar";

			this.NavigationController.ToolbarHidden = true;
			this.NavigationController.NavigationBarHidden = false;

			_homController = new DemoTableController ();

			SetTestButton ();
		}
	}
}

