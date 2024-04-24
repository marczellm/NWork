namespace NWork.MauiCursor;

using AppKit;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

public static class CursorExtensions
{
    public static void SetCustomCursor(this VisualElement visualElement, CursorIcon cursor, IMauiContext? mauiContext)
    {
        ArgumentNullException.ThrowIfNull(mauiContext);
        var view = visualElement.ToPlatform(mauiContext);
        if (view.GestureRecognizers is not null)
        {
            foreach (var recognizer in view.GestureRecognizers.OfType<PointerUIHoverGestureRecognizer>())
            {
                view.RemoveGestureRecognizer(recognizer);
            }
        }

        view.AddGestureRecognizer(new PointerUIHoverGestureRecognizer(r =>
        {
            switch (r.State)
            {
                case UIGestureRecognizerState.Began:
                    GetNSCursor(cursor).Set();
                    break;
                case UIGestureRecognizerState.Ended:
                    NSCursor.ArrowCursor.Set();
                    break;
            }
        }));
    }

	static NSCursor getUndocumentedCursor(string cursorType)
	{
		var nsCursor = Runtime.GetNSObject(Class.GetHandle(nameof(NSCursor)));
		return (NSCursor) nsCursor!.PerformSelector(new Selector(cursorType));
	}

	static NSCursor GetNSCursor(CursorIcon cursor)
    {
        return cursor switch
        {
            CursorIcon.Hand => NSCursor.OpenHandCursor,
            CursorIcon.IBeam => NSCursor.IBeamCursor,
            CursorIcon.Cross => NSCursor.CrosshairCursor,
            CursorIcon.Arrow => NSCursor.ArrowCursor,
            CursorIcon.Move => getUndocumentedCursor("_moveCursor"),
			CursorIcon.ResizeUp => NSCursor.ResizeUpCursor,
			CursorIcon.ResizeDown => NSCursor.ResizeDownCursor,
			CursorIcon.ResizeUpDown => NSCursor.ResizeUpDownCursor,
            CursorIcon.ResizeLeftRight => NSCursor.ResizeLeftRightCursor,
            CursorIcon.Wait => NSCursor.OperationNotAllowedCursor,
            _ => NSCursor.ArrowCursor,
        };
    }

    class PointerUIHoverGestureRecognizer(Action<UIHoverGestureRecognizer> action) : UIHoverGestureRecognizer(action);
}