
using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

namespace FPPopoverSharp
{
	public class FPPopoverController : UIViewController
	{
		FPTouchView _touchView;
		FPPopoverView _contentView;
		UIViewController _viewController;
		UIWindow _window;
		UIView _parentView;
		UIView _fromView;
		UIDeviceOrientation _deviceOrientation;
		PointF _origin;

		FPPopoverArrowDirection ArrowDirection { get; set; }

		SizeF ContentSize { get; set; }

		PointF Origin { 
			get{ return _origin;} 
			set{ _origin = value;}
		}

		FPPopoverTint Tint {
			get {
				return _contentView.Tint;
			} 
			set {
				_contentView.Tint = value;
				_contentView.SetNeedsDisplay ();
			}
		}

		public FPPopoverController (UIViewController viewController) : base()
		{
			_viewController = viewController;

//			SetParentView ();

			InitViews ();
		}

		private void RemoveObservers ()
		{
			UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications ();
			NSNotificationCenter.DefaultCenter.RemoveObserver (this);
			_viewController.RemoveObserver (this, new NSString ("title"));
		}

		private void AddObservers ()
		{
			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications ();	
			NSNotificationCenter.DefaultCenter.AddObserver (UIDevice.OrientationDidChangeNotification, DeviceOrientationDidChange);
			NSNotificationCenter.DefaultCenter.AddObserver (new NSString ("FPNewPopoverPresented"), WillPresentNewPopover);    
    
    
			_deviceOrientation = UIDevice.CurrentDevice.Orientation;
		}

		protected override void Dispose (bool disposing)
		{
			RemoveObservers ();
			_touchView.Dispose ();
			_viewController.Dispose ();
			_contentView.Dispose ();
//			_window.Dispose ();
//			_parentView.Dispose ();
			Delegate = null;

			base.Dispose (disposing);
		}

		void SetupView ()
		{
			// Si no todos los elementos estan instanciados
			if (_parentView == null || _fromView == null) {
				return;
			}

			View.Frame = new RectangleF (0f, 0f, ParentWidth, ParentHeight);
			_touchView.Frame = View.Bounds;
    
			//view position, size and best arrow direction
			BestArrowDirectionAndFrameFromView (_fromView);

			_contentView.SetNeedsDisplay ();
			_touchView.SetNeedsDisplay ();
		}

//		public override void ViewDidLoad ()
//		{
//			base.ViewDidLoad ();
//
//			//InitViews();
//		}

		/// <summary>
		/// Inits the views. Initialize and load the content view
		/// </summary>
		void InitViews ()
		{
			ArrowDirection = FPPopoverArrowDirection.FPPopoverArrowDirectionAny;
			_viewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_viewController.AddObserver (this, new NSString ("title"), NSKeyValueObservingOptions.New, System.IntPtr.Zero);

			_touchView = new FPTouchView (View.Bounds);

			ContentSize = new SizeF (200, 300); //default size (200x300 originalmente)
			_contentView = new FPPopoverView (new RectangleF (0, 0, ContentSize.Width, ContentSize.Height));
			_contentView.ArrowDirection = FPPopoverArrowDirection.FPPopoverArrowDirectionUp;
			_contentView.Title = _viewController.Title;
			_contentView.ClipsToBounds = false;
			_contentView.AddContentView (_viewController.View);

			_touchView.BackgroundColor = UIColor.Clear;
			_touchView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_touchView.ClipsToBounds = false;

			_touchView.SetTouchedOutsideBlock (delegate {
				DismissPopoverAnimated (true);
			}
			); 

			_touchView.AddSubview (_contentView);

			View.UserInteractionEnabled = true;
			View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			View.ClipsToBounds = false;
			View.AddSubview (_touchView);

			//_parentView.AddSubview (View);

			SetupView ();
			AddObservers (); 
		}
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}

		float ParentWidth {
			get {
				if (_parentView == null) {
					return 0.0f;
				}

				//return _parentView.Bounds.Size.Width;
				return UIDeviceOrientation.Portrait == _deviceOrientation ? _parentView.Frame.Size.Width : _parentView.Frame.Size.Height;
			}
		}

		float ParentHeight {
			get {
				if (_parentView == null) {
					return 0.0f;
				}

				//return _parentView.bounds.size.height;
				return UIDeviceOrientation.Portrait == _deviceOrientation ? _parentView.Frame.Size.Height : _parentView.Frame.Size.Width;
			}
		}

		public void PresentPopoverFromPoint (PointF fromPoint)
		{
			Origin = fromPoint;
//			if (_parentView != null) {
//				_contentView.RelativeOrigin = _parentView.ConvertPointToView (fromPoint, _contentView);
//			} 

			View.RemoveFromSuperview ();
			var windows = UIApplication.SharedApplication.Windows;

			if (windows.Length > 0) {
				if (_window != null) {
					_window.Dispose ();
				}

//				if (_parentView != null) {
//					_parentView.Dispose ();
//					_parentView = null;
//				}

				_window = windows [0];

				//keep the first subview
				if (_window.Subviews.Length > 0) {
					//_parentView.Dispose ();
					_parentView = _window.Subviews [0];
					_parentView.AddSubview (View);
				}
			} else {
				DismissPopoverAnimated (false);
			}

			SetupView ();
			View.Alpha = 0.0f;
			UIView.Animate (0.2d, delegate {         
				View.Alpha = 1.0f;
			}
			);
    
			NSNotificationCenter.DefaultCenter.PostNotificationName ("FPNewPopoverPresented", this);
		}

		PointF OriginFromView (UIView fromView)
		{
			PointF p;
			if (_contentView.ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
				p.X = fromView.Frame.Location.X + fromView.Frame.Size.Width / 2.0f;
				p.Y = fromView.Frame.Location.Y + fromView.Frame.Size.Height;
			} else if (_contentView.ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionDown) {
				p.X = fromView.Frame.Location.X + fromView.Frame.Size.Width / 2.0f;
				p.Y = fromView.Frame.Location.Y;        
			} else if (_contentView.ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionLeft) {
				p.X = fromView.Frame.Location.X + fromView.Frame.Size.Width;
				p.Y = fromView.Frame.Location.Y + fromView.Frame.Size.Height / 2.0f;
			} else if (_contentView.ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionRight) {
				p.X = fromView.Frame.Location.X;
				p.Y = fromView.Frame.Location.Y + fromView.Frame.Size.Height / 2.0f;
			}

			return p;
		}

		public void PresentPopoverFromView (UIView fromView)
		{
			if (fromView == null) {
				throw new ArgumentException ("fromView no puede ser nulo");
			}

			if (_fromView != null) {
				_fromView.Dispose ();
			}
			_fromView = fromView;
			PresentPopoverFromPoint (OriginFromView (_fromView));
		}

		void DismissPopover ()
		{
			View.RemoveFromSuperview ();

			if (Delegate != null && Delegate.RespondsToSelector (new Selector ("PopoverControllerDidDismissPopover"))) {
				Delegate.PopoverControllerDidDismissPopover (this);
			}

			//    if(Delegate respondsToSelector:@selector(popoverControllerDidDismissPopover:)])
			//    {
			//        [self.delegate popoverControllerDidDismissPopover:self];
			//    }

//			_window.Dispose ();
//			_window = null;
//			_parentView.Dispose ();
//			_parentView = null;
		}

		FPPopoverControllerDelegate Delegate {
//			get { return WeakDelegate;}
//			set{ WeakDelegate = value;}
			get;
			set;
		}		

//		NSObject WeakDelegate { 
////			get { return Delegate;}
////			set{ Delegate = value;}
//			get;
//			set;
//		}

		void DismissPopoverAnimated (bool animated)
		{
			if (animated) {
				UIView.Animate (0.2d, delegate {
					View.Alpha = 0.0f;
				}, delegate {
					DismissPopover ();
				}
				);
			} else {
				DismissPopover ();
			}
		}

		void DeviceOrientationDidChange (NSNotification notification)
		{
			_deviceOrientation = UIDevice.CurrentDevice.Orientation;
    
			UIView.Animate (0.2, delegate {
				SetupView (); 
			}
			);
		}

		void WillPresentNewPopover (NSNotification notification)
		{
			if (notification.Object != this) {
				if (Delegate != null && Delegate.RespondsToSelector (new Selector ("PresentedNewPopoverControllershouldDismissVisiblePopover"))) {
					Delegate.PresentedNewPopoverControllershouldDismissVisiblePopover ((FPPopoverController)notification.Object, this);
				}
			}
		}

		public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			//base.ObserveValue (keyPath, ofObject, change, context);
			if (ofObject == _viewController && keyPath.Equals ("title")) {
				_contentView.Title = _viewController.Title;
				_contentView.SetNeedsDisplay ();
			}
		}




//		/// <summary>
//		/// This methods helps the controller to found a proper way to display the view.
//		/// If the "from point" will be on the left, the arrow will be on the left and the 
//		/// view will be move on the right of the from point.
//		/// Consider only x direction
//		///  |--lm--|-----s-----|--rm--|
//		/// s is the frame of our view (s < screen width). 
//		/// if our origin point is in lm or rm we move s
//		/// if the origin point is in s we move the arrow 
//		/// </summary>
//		/// <returns>
//		/// The view frame for from point.
//		/// </returns>
//		/// <param name='point'>
//		/// Point.
//		/// </param>
//		RectangleF BestViewFrameForFromPoint (PointF point)
//		{
//			//content view size
//			RectangleF r;
//			r.Size = ContentSize;
//			r.Size.Width += 20;
//			r.Size.Height += 50;
//
//			var rLocation = r.Location;
//    
//			//size limits
//			var w = Math.Min (r.Size.Width, ParentWidth);
//			var h = Math.Min (r.Size.Height, ParentHeight);
//    
//			r.Size.Width = (w == ParentWidth) ? ParentWidth - 50 : w;
//			r.Size.Height = (h == ParentHeight) ? ParentHeight - 30 : h;
//    
//			var r_w = r.Size.Width;
//			var r_h = r.Size.Height;
//    
//			//lm + rm
//			var wm = ParentWidth - r_w;
//			var wm_l = wm / 2.0f;
//			var ws = r_w;
//			var rm_x = wm_l + ws;
//    
//			var hm = ParentHeight - r_h;
//			var hm_t = hm / 2.0f; //top
//			var hs = r_h;
//			var hm_b = hm_t + hs; //bottom
//    
//			if (wm > 0) {
//				//s < lm + rm
//				//our content size is smaller then width
//        
//				//15px are the number of point from the border to the arrow when the
//				//arrow is totally at left
//				//I have considered a standard border of 2px
//
//				if (point.X + 15 <= wm_l) {
//					//move the popup to the left, with the left side near the origin point
//					rLocation.X = point.X - 15;
//				} else if (point.X + 15 >= rm_x) {
//					//move the popup to the right, with the right side near the origin point
//					rLocation.X = point.X - ws + 22;
//				} else {
//					//the point is in the "s" zone and then I will move only the arrow
//					//put in the x center the popup
//					rLocation.X = wm_l;
//				}
//			}
//    
//    
//			if (hm > 0) {
//				//the point is on the top
//				//let's move up the view
//				if (point.Y <= hm_t) {
//					rLocation.Y = point.Y;            
//				}
//        		//the point is on the bottom, 
//        //let's move down the view
//        		else if (point.Y > hm_b) {
//					rLocation.Y = point.Y - hs;
//				} else {
//					//we need to resize the content
//					rLocation.Y = point.Y;
//					r.Size.Height = Math.Min (ContentSize.Height, ParentHeight - point.Y - 10); //resizing
//				}
//			}
//
//			r.Location = rLocation;
//			//r.Size = rSize;
//    
//			return r;
//		}

		/// <summary>
		/// View position, size and best arrow direction
		/// </summary>
		/// <returns>
		/// The arrow direction and frame from view.
		/// </returns>
		/// <param name='v'>
		/// V.
		/// </param>
		RectangleF BestArrowDirectionAndFrameFromView (UIView v)
		{
			var p = v.Superview.ConvertPointToView (v.Frame.Location, View);

			var ht = p.Y; //available vertical space on top of the view
			var hb = ParentHeight - (p.Y + v.Frame.Size.Height); //on the bottom
			var wl = p.X; //on the left
			var wr = ParentWidth - (p.X + v.Frame.Size.Width); //on the right
        
			var best_h = Math.Max (ht, hb); //much space down or up ?
			var best_w = Math.Max (wl, wr);
    
			//RectangleF r;
			var rSize = ContentSize;
			PointF rLocation = new PointF ();

			FPPopoverArrowDirection bestDirection;
    
			//if the user wants vertical arrow, check if the content will fit vertically 
			if (ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionVertical || 
				(ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionAny && best_h >= best_w)) {

				//ok, will be vertical
				if (ht == best_h || ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionDown) {
					//on the top and arrow down
					bestDirection = FPPopoverArrowDirection.FPPopoverArrowDirectionDown;
            
					// BUG: creo que location.X no toma el valor, no se puede setear
					rLocation.X = p.X + (v.Frame.Size.Width / 2.0f) - (rSize.Width / 2.0f);
					rLocation.Y = p.Y - rSize.Height;
				} else {
					//on the bottom and arrow up
					bestDirection = FPPopoverArrowDirection.FPPopoverArrowDirectionUp;

					rLocation.X = p.X + v.Frame.Size.Width / 2.0f - rSize.Width / 2.0f;
					rLocation.Y = p.Y + v.Frame.Size.Height;
				}
			} else {
				//ok, will be horizontal 
				if (wl == best_w || ArrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionRight) {
					//on the left and arrow right
					bestDirection = FPPopoverArrowDirection.FPPopoverArrowDirectionRight;

					rLocation.X = p.X - rSize.Width;
					rLocation.Y = p.Y + v.Frame.Size.Height / 2.0f - rSize.Height / 2.0f;

				} else {
					//on the right then arrow left
					bestDirection = FPPopoverArrowDirection.FPPopoverArrowDirectionLeft;

					rLocation.X = p.X + v.Frame.Size.Width;
					rLocation.Y = p.Y + v.Frame.Size.Height / 2.0f - rSize.Height / 2.0f;
				}
			}
    
			//need to moved left ? 
			if (rLocation.X + rSize.Width > ParentWidth) {
				rLocation.X = ParentWidth - rSize.Width;
			}    
    		//need to moved right ?
    		else if (rLocation.X < 0) {
				rLocation.X = 0;
			}    
    
			//need to move up?
			if (rLocation.Y < 0) {
				var old_y = rLocation.Y;
				rLocation.Y = 0;
				rSize.Height += old_y;
			}
    
			//need to be resized horizontally ?
			if (rLocation.X + rSize.Width > ParentWidth) {
				rSize.Width = ParentWidth - rLocation.X;
			}
    
			//need to be resized vertically ?
			if (rLocation.Y + rSize.Height > ParentHeight) {
				rSize.Height = ParentHeight - rLocation.Y;
			}
    
    
			if (!UIApplication.SharedApplication.StatusBarHidden) {
				if (rLocation.Y <= 20) {
					rLocation.Y += 20;
				}
			}

			_contentView.ArrowDirection = bestDirection;

			var r = new RectangleF (rLocation, rSize);
			_contentView.Frame = r;

			Origin = new PointF (p.X + v.Frame.Size.Width / 2.0f, p.Y + v.Frame.Size.Height / 2.0f);
			_contentView.RelativeOrigin = _parentView.ConvertPointToView (Origin, _contentView);

			return r;
		}
	}

}

