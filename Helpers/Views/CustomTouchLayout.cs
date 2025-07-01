using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;

namespace PlayTube.Helpers.Views
{
	public class CustomTouchLayout : FrameLayout
	{
		public bool IsTouch = true;

		protected CustomTouchLayout(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public CustomTouchLayout([NotNull] Context context) : base(context)
		{
			IsTouch = true;
		}

		public CustomTouchLayout([NotNull] Context context, IAttributeSet attrs) : base(context, attrs)
		{
			IsTouch = true;
		}

		public CustomTouchLayout([NotNull] Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			IsTouch = true;
		}

		public CustomTouchLayout([NotNull] Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
			IsTouch = true;
		}

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			//if (!OnInterceptTouchEvent(e))
			//{

			//    for (View child : ChildCount)
			//    {
			//        if (child.dispatchTouchEvent(ev))
			//            return true;
			//    }
			//}
			//return super.dispatchTouchEvent(ev);
			return base.DispatchTouchEvent(e);
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			//if (ev.Action == MotionEventActions.Down)
			//{
			//    x = ev.RawX;
			//    return false;
			//}
			//else if (ev.Action == MotionEventActions.Move)
			//{
			//    //var d = ev.GetX();
			//    //var d2 = ev.RawX;
			//    //if (d>400)
			//    //  return true;
			//    //else
			//    return true;
			//}
			//else
			//{
			//    return true;
			//}
			return true;
			//return base.OnInterceptTouchEvent(ev);
		}

		public void SetOnInterceptTouch(bool canTouch)
		{
			IsTouch = canTouch;
		}

	}
}