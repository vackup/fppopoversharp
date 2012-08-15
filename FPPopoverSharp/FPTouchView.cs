using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace FPPopoverSharp
{
	public class FPTouchView : UIView
	{
		private FPTouchedOutsideBlock _outsideBlock;
		private FPTouchedInsideBlock  _insideBlock;

		public delegate void FPTouchedOutsideBlock ();
		public delegate void FPTouchedInsideBlock ();

		public FPTouchView (RectangleF frame) : base (frame)
		{

		}

		protected override void Dispose (bool disposing)
		{
			_outsideBlock = null;
			_insideBlock = null;
			base.Dispose (disposing);
		}

		public void SetTouchedOutsideBlock (FPTouchedOutsideBlock outsideBlock)
		{
			_outsideBlock = outsideBlock;
		}

		public void SetTouchedInsideBlock (FPTouchedInsideBlock insideBlock)
		{
			_insideBlock = insideBlock;
		}

		public override UIView HitTest (PointF point, UIEvent uievent)
		{
			var subview = base.HitTest (point, uievent);

			if (UIEventType.Touches == uievent.Type) {
				var touchedInside = subview != this;

				if (!touchedInside) {
					foreach (var s in Subviews) {
						if (s == subview) {
							touchedInside = true;
							break;
						}
					}
				}

				if (touchedInside && _insideBlock != null) {
					_insideBlock ();
				} else if (!touchedInside && _outsideBlock != null) {
					_outsideBlock ();
				}
			}

			return subview;
		}

//		private UIView HitTest(CGPoint point){
//
//		}
	}
}

