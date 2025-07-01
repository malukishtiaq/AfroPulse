using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Helpers.ShimmerUtils
{
	public class ShimmerTemplateLayout : LinearLayout
	{
		private int TemplateType;

		protected ShimmerTemplateLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public ShimmerTemplateLayout(Context context) : base(context)
		{
			//InitView(context, null);
		}

		public ShimmerTemplateLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			//InitView(context, attrs);
		}

		public ShimmerTemplateLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			//InitView(context, attrs);
		}

		public ShimmerTemplateLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
			//InitView(context, attrs);
		}

		private void InitView(Context context, IAttributeSet attributeSet)
		{
			TypedArray attributes = null;
			try
			{
				if (attributeSet != null)
				{
					attributes = context?.Theme?.ObtainStyledAttributes(attributeSet, Resource.Styleable.TemplateView, 0, 0);
					TemplateType = attributes.GetResourceId(Resource.Styleable.TemplateShimmer_template_res, Resource.Layout.Style_RowShimmerView);
					LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
					inflater.Inflate(TemplateType, this);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
			finally
			{
				attributes?.Recycle();
			}
		}

		public void SetTemplateStyle(ShimmerTemplateStyle style)
		{
			try
			{
				LayoutInflater inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
				switch (style)
				{
					case ShimmerTemplateStyle.VideoRowTemplate:
						TemplateType = Resource.Layout.Style_RowShimmerView;
						break;
					case ShimmerTemplateStyle.VideoBigTemplate:
						TemplateType = Resource.Layout.Style_BigShimmerView;
						break;
					case ShimmerTemplateStyle.ChannelTemplate:
						TemplateType = Resource.Layout.Style_ChannelShimmerView;
						break;
					case ShimmerTemplateStyle.NotificationTemplate:
						TemplateType = Resource.Layout.Style_NotificationShimmerView;
						break;
					case ShimmerTemplateStyle.MoviesTemplate:
						TemplateType = Resource.Layout.Style_MoviesShimmerView;
						break;
				}

				inflater.Inflate(TemplateType, this);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public ShimmerTemplateStyle GetTemplateTypeName()
		{
			switch (TemplateType)
			{
				case Resource.Layout.Style_RowShimmerView:
					return ShimmerTemplateStyle.VideoRowTemplate;
				case Resource.Layout.Style_BigShimmerView:
					return ShimmerTemplateStyle.VideoBigTemplate;
				case Resource.Layout.Style_ChannelShimmerView:
					return ShimmerTemplateStyle.ChannelTemplate;
				case Resource.Layout.Style_NotificationShimmerView:
					return ShimmerTemplateStyle.NotificationTemplate;
				case Resource.Layout.Style_MoviesShimmerView:
					return ShimmerTemplateStyle.MoviesTemplate;
				default:
					return ShimmerTemplateStyle.VideoRowTemplate;
			}
		}

	}
}