using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace FPPopoverSharp
{

	public abstract class FPPopoverControllerDelegate : NSObject
	{
		public abstract void PopoverControllerDidDismissPopover (FPPopoverController popoverController);

		public abstract	void PresentedNewPopoverControllershouldDismissVisiblePopover (FPPopoverController newPopoverController, FPPopoverController visiblePopoverController);

	}
}
