using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;

namespace FPPopoverSharp
{
	public class FPPopoverView : UIView
	{
		FPPopoverArrowDirection _arrowDirection;
		UIView _contentView;
		UILabel _titleLabel;

		const float FP_POPOVER_ARROW_HEIGHT = 15.0f;
		const float FP_POPOVER_ARROW_BASE = 20.0f;
		const float FP_POPOVER_RADIUS = 10.0f;

		public FPPopoverView (RectangleF frame) : base(frame)
		{
			//we need to set the background as clear to see the view below
			BackgroundColor = UIColor.Clear;
			ClipsToBounds = true;
        
			Layer.ShadowOpacity = 0.7f;
			Layer.ShadowRadius = 5.0f;
			Layer.ShadowOffset = new SizeF (-3, 3);

			//to get working the animations
			ContentMode = UIViewContentMode.Redraw;        
        
			_titleLabel = new UILabel ();
			_titleLabel.BackgroundColor = UIColor.Clear;
			_titleLabel.TextColor = UIColor.White;
			_titleLabel.TextAlignment = UITextAlignment.Center;
			_titleLabel.Font = UIFont.FromName ("HelveticaNeue-Bold", 16f);
        
			Tint = FPPopoverTint.FPPopoverDefaultTint;
        
			AddSubview (_titleLabel);
			SetupViews ();
		}

		public PointF RelativeOrigin { get; set; }

		public FPPopoverTint Tint { get; set; }

		public string Title { get; set; }

		protected override void Dispose (bool disposing)
		{
			Title = null;
			_contentView.Dispose ();
			_titleLabel.Dispose ();
			base.Dispose (disposing);
		}

		public void AddContentView (UIView contentView)
		{
			if (_contentView == null || _contentView != contentView) {
				if (_contentView != null) {
					_contentView.RemoveFromSuperview ();
					_contentView.Dispose ();
				}
				_contentView = contentView;
				AddSubview (_contentView);
			}
			SetupViews ();
		}

		public FPPopoverArrowDirection ArrowDirection {
			get { 
				return _arrowDirection;
			}
			set {
				_arrowDirection = value;
				this.SetNeedsDisplay ();
			}
		}

		private CGPath NewContentPathWithBorderWidth (float borderWidth, FPPopoverArrowDirection direction)
		{
			float w = Bounds.Size.Width;
			float h = Bounds.Size.Height;
			float ah = FP_POPOVER_ARROW_HEIGHT; //is the height of the triangle of the arrow
			float aw = FP_POPOVER_ARROW_BASE / 2.0f; //is the 1/2 of the base of the arrow
			float radius = FP_POPOVER_RADIUS;
			float b = borderWidth;

			//RectangleF rect;
			SizeF rectSize = new SizeF ();
			PointF rectLocation = new PointF ();

			if (direction == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {        
				rectSize.Width = w - 2f * b;
				rectSize.Height = h - ah - 2f * b;
				rectLocation.X = b;
				rectLocation.Y = ah + b;        
			} else if (direction == FPPopoverArrowDirection.FPPopoverArrowDirectionDown) {
				rectSize.Width = w - 2f * b;
				rectSize.Height = h - ah - 2f * b;
				rectLocation.X = b;
				rectLocation.Y = b;                
			} else if (direction == FPPopoverArrowDirection.FPPopoverArrowDirectionRight) {
				rectSize.Width = w - ah - 2f * b;
				rectSize.Height = h - 2f * b;
				rectLocation.X = b;
				rectLocation.Y = b;                
			} else {
				//Assuming direction == FPPopoverArrowDirectionLeft to suppress static analyzer warnings
				rectSize.Width = w - ah - 2f * b;
				rectSize.Height = h - 2f * b;
				rectLocation.X = ah + b;
				rectLocation.Y = b;                
			}

			//the arrow will be near the origin
			float ax = RelativeOrigin.X - aw; //the start of the arrow when UP or DOWN
			if (ax < aw + b) {
				ax = aw + b;
			} else if (ax + 2 * aw + 2 * b > Bounds.Size.Width) {
				ax = Bounds.Size.Width - 2 * aw - 2 * b;
			}

			float ay = RelativeOrigin.Y - aw; //the start of the arrow when RIGHT or LEFT
			if (ay < aw + b) {
				ay = aw + b;
			} else if (ay + 2f * aw + 2f * b > Bounds.Size.Height) {
				ay = Bounds.Size.Height - 2f * aw - 2f * b;
			}

			//ROUNDED RECT
			// arrow UP
			var rect = new RectangleF (rectLocation, rectSize);

			var innerRect = RectangleF.Inflate (rect, -radius, -radius);
			float inside_right = innerRect.Location.X + innerRect.Size.Width;
			float outside_right = rect.Location.X + rect.Size.Width;
			float inside_bottom = innerRect.Location.Y + innerRect.Size.Height;
			float outside_bottom = rect.Location.Y + rect.Size.Height;    
			float inside_top = innerRect.Location.Y;
			float outside_top = rect.Location.Y;
			float outside_left = rect.Location.X;

			//drawing the border with arrow
			var path = new CGPath ();

			path.MoveToPoint (innerRect.Location.X, outside_top);

			//top arrow
			if (direction == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
				path.AddLineToPoint (ax, ah + b);
				path.AddLineToPoint (ax + aw, b);
				path.AddLineToPoint (ax + 2f * aw, ah + b);
        
			}

			path.AddLineToPoint (inside_right, outside_top);
			path.AddArcToPoint (outside_right, outside_top, outside_right, inside_top, radius);

			//right arrow
			if (direction == FPPopoverArrowDirection.FPPopoverArrowDirectionRight) {
				path.AddLineToPoint (outside_right, ay);
				path.AddLineToPoint (outside_right + ah + b, ay + aw);
				path.AddLineToPoint (outside_right, ay + 2f * aw);
			}
       

			path.AddLineToPoint (outside_right, inside_bottom);
			path.AddArcToPoint (outside_right, outside_bottom, inside_right, outside_bottom, radius);

			//down arrow
			if (direction == FPPopoverArrowDirection.FPPopoverArrowDirectionDown) {
				path.AddLineToPoint (ax + 2f * aw, outside_bottom);
				path.AddLineToPoint (ax + aw, outside_bottom + ah);
				path.AddLineToPoint (ax, outside_bottom);
			}

			path.AddLineToPoint (innerRect.Location.X, outside_bottom);
			path.AddArcToPoint (outside_left, outside_bottom, outside_left, inside_bottom, radius);
    
			//left arrow
			if (direction == FPPopoverArrowDirection.FPPopoverArrowDirectionLeft) {
				path.AddLineToPoint (outside_left, ay + 2f * aw);
				path.AddLineToPoint (outside_left - ah - b, ay + aw);
				path.AddLineToPoint (outside_left, ay);
			}    

			path.AddLineToPoint (outside_left, inside_top);
			path.AddArcToPoint (outside_left, outside_top, innerRect.Location.X, outside_top, radius);    
			path.CloseSubpath ();
    
			return path;
		}

		private CGGradient NewGradient ()
		{
			var colorSpace = CGColorSpace.CreateDeviceRGB ();

			// make a gradient    
			var colors = new float[8];
    
			if (Tint == FPPopoverTint.FPPopoverBlackTint) {
				if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
					colors [0] = colors [1] = colors [2] = 0.6f;
					colors [4] = colors [5] = colors [6] = 0.1f;
					colors [3] = colors [7] = 1.0f;
				} else {
					colors [0] = colors [1] = colors [2] = 0.4f;
					colors [4] = colors [5] = colors [6] = 0.1f;
					colors [3] = colors [7] = 1.0f;
				}        
			} else if (Tint == FPPopoverTint.FPPopoverLightGrayTint) {
				if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
					colors [0] = colors [1] = colors [2] = 0.8f;
					colors [4] = colors [5] = colors [6] = 0.3f;
					colors [3] = colors [7] = 1.0f;
				} else {
					colors [0] = colors [1] = colors [2] = 0.6f;
					colors [4] = colors [5] = colors [6] = 0.1f;
					colors [3] = colors [7] = 1.0f;
				}        
			} else if (Tint == FPPopoverTint.FPPopoverRedTint) {
				if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
					colors [0] = 0.72f;
					colors [1] = 0.35f;
					colors [2] = 0.32f;
					colors [4] = 0.36f;
					colors [5] = 0.0f;
					colors [6] = 0.09f;
					colors [3] = colors [7] = 1.0f;

				} else {
					colors [0] = 0.82f;
					colors [1] = 0.45f;
					colors [2] = 0.42f;
					colors [4] = 0.36f;
					colors [5] = 0.0f;
					colors [6] = 0.09f;
					colors [3] = colors [7] = 1.0f;
				}        
			} else if (Tint == FPPopoverTint.FPPopoverGreenTint) {
				if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
					colors [0] = 0.35f;
					colors [1] = 0.72f;
					colors [2] = 0.17f;
					colors [4] = 0.18f;
					colors [5] = 0.30f;
					colors [6] = 0.03f;
					colors [3] = colors [7] = 1.0f;
            
				} else {
					colors [0] = 0.45f;
					colors [1] = 0.82f;
					colors [2] = 0.27f;
					colors [4] = 0.18f;
					colors [5] = 0.30f;
					colors [6] = 0.03f;
					colors [3] = colors [7] = 1.0f;
				}        
			}
    
    

			var gradient = new CGGradient (colorSpace, colors);
			//CFRelease (colorSpace);

			return gradient;
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);
			
			var gradient = NewGradient ();    
    
			var ctx = UIGraphics.GetCurrentContext ();  
			ctx.SaveState ();
        
			//content fill
			var contentPath = NewContentPathWithBorderWidth (2.0f, _arrowDirection);
   
			ctx.AddPath (contentPath);    
			ctx.Clip ();

			//  Draw a linear gradient from top to bottom
			PointF start;
			PointF end;
			if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
				start = new PointF (Bounds.Size.Width / 2.0f, 0f);
				end = new PointF (Bounds.Size.Width / 2.0f, 40f);
			} else {
				start = new PointF (Bounds.Size.Width / 2.0f, 0f);
				end = new PointF (Bounds.Size.Width / 2.0f, 20f);
			}
    
			ctx.DrawLinearGradient (gradient, start, end, 0);
    
			//CGGradientRelease(gradient);
			gradient.Dispose ();

			//fill the other part of path
			if (Tint == FPPopoverTint.FPPopoverBlackTint) {
				ctx.SetFillColor (0.1f, 0.1f, 0.1f, 1.0f);        
			} else if (Tint == FPPopoverTint.FPPopoverLightGrayTint) {
				ctx.SetFillColor (0.3f, 0.3f, 0.3f, 1.0f);        
			} else if (Tint == FPPopoverTint.FPPopoverRedTint) {
				ctx.SetFillColor (0.36f, 0.0f, 0.09f, 1.0f);        
			} else if (Tint == FPPopoverTint.FPPopoverGreenTint) {
				ctx.SetFillColor (0.18f, 0.30f, 0.03f, 1.0f);        
			}

    
			ctx.FillRect (new RectangleF (0, end.Y, Bounds.Size.Width, Bounds.Size.Height - end.Y));
			//internal border
			ctx.BeginPath ();
			ctx.AddPath (contentPath);
			ctx.SetStrokeColor (0.7f, 0.7f, 0.7f, 1.0f);
			ctx.SetLineWidth (1f);
			ctx.SetLineCap (CGLineCap.Round);
			ctx.SetLineJoin (CGLineJoin.Round);
			ctx.StrokePath ();

			contentPath.Dispose ();
			//CGPathRelease(contentPath);

			//external border
			var externalBorderPath = NewContentPathWithBorderWidth (1.0f, _arrowDirection);
			ctx.BeginPath ();
			ctx.AddPath (externalBorderPath);
			ctx.SetStrokeColor (0.4f, 0.4f, 0.4f, 1.0f);
			ctx.SetLineWidth (1f);
			ctx.SetLineCap (CGLineCap.Round);
			ctx.SetLineJoin (CGLineJoin.Round);
			ctx.StrokePath ();

			externalBorderPath.Dispose ();
			//CGPathRelease (externalBorderPath);

			//3D border of the content view
			//var cvRect = _contentView.Frame;
			var cvRectLocation = _contentView.Frame.Location;
			var cvRectSize = _contentView.Frame.Size;
			//firstLine
			ctx.SetStrokeColor (0.7f, 0.7f, 0.7f, 1.0f);
			ctx.StrokeRect (new RectangleF (cvRectLocation, cvRectSize));
			//secondLine
			cvRectLocation.X -= 1f;
			cvRectLocation.Y -= 1f;
			cvRectSize.Height += 2f;
			cvRectSize.Width += 2f;
			ctx.SetStrokeColor (0.4f, 0.4f, 0.4f, 1.0f);
			ctx.StrokeRect (new RectangleF (cvRectLocation, cvRectSize));
    
      
			ctx.RestoreState ();
		}

		private void SetupViews ()
		{
			// Si todavia los elementos no se instanciaron
			if (_contentView == null || _titleLabel == null) {
				return;
			}

			//content posizion and size
			var contentRect = _contentView.Frame;

			if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionUp) {
				contentRect.Location = new PointF (10, 60);  
				contentRect.Size = new SizeF (Bounds.Size.Width - 20, Bounds.Size.Height - 70);
				_titleLabel.Frame = new RectangleF (10, 30, Bounds.Size.Width - 20, 20);    
				if (Title == null || Title.Length == 0) {
					contentRect.Location = new PointF (10, 30);
					contentRect.Size = new SizeF (Bounds.Size.Width - 20, Bounds.Size.Height - 40);
				}
			} else if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionDown) {
				contentRect.Location = new PointF (10, 40);        
				contentRect.Size = new SizeF (Bounds.Size.Width - 20, Bounds.Size.Height - 70);
				_titleLabel.Frame = new RectangleF (10, 10, Bounds.Size.Width - 20, 20);
				if (Title == null || Title.Length == 0) {
					contentRect.Location = new PointF (10, 10); 
					contentRect.Size = new SizeF (Bounds.Size.Width - 20, Bounds.Size.Height - 40);
				}
			} else if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionRight) {
				contentRect.Location = new PointF (10, 40);        
				contentRect.Size = new SizeF (Bounds.Size.Width - 40, Bounds.Size.Height - 50);
				_titleLabel.Frame = new RectangleF (10, 10, Bounds.Size.Width - 20, 20);    
				if (Title == null || Title.Length == 0) {
					contentRect.Location = new PointF (10, 10);
					contentRect.Size = new SizeF (Bounds.Size.Width - 40, Bounds.Size.Height - 20);
				}
			} else if (_arrowDirection == FPPopoverArrowDirection.FPPopoverArrowDirectionLeft) {
				contentRect.Location = new PointF (10 + FP_POPOVER_ARROW_HEIGHT, 40);        
				contentRect.Size = new SizeF (Bounds.Size.Width - 40, Bounds.Size.Height - 50);
				_titleLabel.Frame = new RectangleF (10, 10, Bounds.Size.Width - 20, 20); 
				if (Title == null || Title.Length == 0) {
					contentRect.Location = new PointF (10 + FP_POPOVER_ARROW_HEIGHT, 10);
					contentRect.Size = new SizeF (Bounds.Size.Width - 40, Bounds.Size.Height - 20);
				}
			}

			_contentView.Frame = contentRect;
			_titleLabel.Text = Title;  
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			SetupViews ();
		}

		public override RectangleF Frame {
			get {
				return base.Frame;
			}
			set {
				base.Frame = value;
				SetupViews ();
			}
		}

		public override RectangleF Bounds {
			get {
				return base.Bounds;
			}
			set {
				base.Bounds = value;
				SetupViews ();
			}
		}
	}
}

