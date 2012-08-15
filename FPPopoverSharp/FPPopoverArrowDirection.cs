using System;

namespace FPPopoverSharp
{

	public enum FPPopoverArrowDirection
	{
		FPPopoverArrowDirectionUp = 0,
		FPPopoverArrowDirectionDown = 1,
		FPPopoverArrowDirectionLeft = 2,
		FPPopoverArrowDirectionRight = 3,
		FPPopoverArrowDirectionVertical = FPPopoverArrowDirectionUp | FPPopoverArrowDirectionDown,
		FPPopoverArrowDirectionHorizontal = FPPopoverArrowDirectionLeft | FPPopoverArrowDirectionRight,
    
		FPPopoverArrowDirectionAny = FPPopoverArrowDirectionUp | FPPopoverArrowDirectionDown | 
			FPPopoverArrowDirectionLeft | FPPopoverArrowDirectionRight
	}
	 
}
