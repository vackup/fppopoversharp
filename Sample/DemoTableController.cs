using System;
using MonoTouch.UIKit;

namespace Sample
{
	public class DemoTableController : UITableViewController
	{
		public DemoTableController ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.Title = "Popover sharp";

			var image = UIImage.FromFile ("Images/cat1.png");

			var uiImageView = new UIImageView (image);

			View.AddSubview (uiImageView);
		}

//		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
//		{
//			return (toInterfaceOrientation == UIInterfaceOrientation.Portrait);
//		}
	}
}

