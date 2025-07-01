using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using System;

namespace PlayTube.Helpers.Utils
{
	public class TemplateRecyclerInflater
	{
		public LinearLayout MainLinear;
		public TextView TitleText, MoreText;
		public ImageView TitleIcon;
		public RecyclerView Recyler;
		public dynamic LayoutManager;

		public enum TypeLayoutManager
		{
			LinearLayoutManagerVertical,
			LinearLayoutManagerHorizontal,
			GridLayoutManagerVertical,
			GridLayoutManagerHorizontal,
			StaggeredGridLayoutManagerVertical,
			StaggeredGridLayoutManagerHorizontal
		}

		private void InitComponent(View inflated)
		{
			try
			{
				MainLinear = inflated.FindViewById<LinearLayout>(Resource.Id.mainLinear);
				TitleIcon = inflated.FindViewById<ImageView>(Resource.Id.iconTitle);
				TitleText = inflated.FindViewById<TextView>(Resource.Id.textTitle);
				MoreText = inflated.FindViewById<TextView>(Resource.Id.textMore);
				Recyler = inflated.FindViewById<RecyclerView>(Resource.Id.recyler);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void InflateLayout<T>(Activity activity, View inflated, dynamic mAdapter, TypeLayoutManager manager, int spanCount = 0, bool showTitle = true, string titleText = "", int titleIcon = 0, bool showMore = false) where T : class
		{
			try
			{
				InitComponent(inflated);

				if (showTitle)
				{
					MainLinear.Visibility = ViewStates.Visible;

					if (string.IsNullOrEmpty(titleText))
					{
						TitleText.Visibility = ViewStates.Gone;
					}
					else
					{
						TitleText.Text = titleText;
					}

					if (titleIcon > 0)
					{
						TitleIcon.SetImageResource(titleIcon);
					}

					if (showMore)
						MoreText.Visibility = ViewStates.Visible;
					else
						MoreText.Visibility = ViewStates.Gone;
				}
				else
					MainLinear.Visibility = ViewStates.Gone;

				switch (manager)
				{
					case TypeLayoutManager.LinearLayoutManagerHorizontal:
						LayoutManager = new LinearLayoutManager(activity, LinearLayoutManager.Horizontal, false);
						Recyler.NestedScrollingEnabled = false;
						break;
					case TypeLayoutManager.LinearLayoutManagerVertical:
						LayoutManager = new LinearLayoutManager(activity);
						break;
					case TypeLayoutManager.GridLayoutManagerVertical:
						LayoutManager = new GridLayoutManager(activity, spanCount);
						break;
					case TypeLayoutManager.GridLayoutManagerHorizontal:
						LayoutManager = new GridLayoutManager(activity, spanCount, LinearLayoutManager.Horizontal, false);
						Recyler.NestedScrollingEnabled = false;
						break;
					case TypeLayoutManager.StaggeredGridLayoutManagerVertical:
						LayoutManager = new StaggeredGridLayoutManager(spanCount, LinearLayoutManager.Vertical);
						break;
					case TypeLayoutManager.StaggeredGridLayoutManagerHorizontal:
						LayoutManager = new StaggeredGridLayoutManager(spanCount, LinearLayoutManager.Horizontal);
						Recyler.NestedScrollingEnabled = false;
						break;
					default:
						LayoutManager = new LinearLayoutManager(activity);
						break;
				}

				Recyler.SetLayoutManager(LayoutManager);
				Recyler.SetItemViewCacheSize(20);
				Recyler.HasFixedSize = true;
				Recyler.SetItemViewCacheSize(10);
				Recyler.GetLayoutManager().ItemPrefetchEnabled = true;

				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<T>(activity, mAdapter, sizeProvider, 10);
				Recyler.AddOnScrollListener(preLoader);

				Recyler.SetAdapter(mAdapter);

				if (Recyler.Visibility != ViewStates.Visible)
					Recyler.Visibility = ViewStates.Visible;

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetTextViewDrawableColor(TextView textView, string color)
		{
			foreach (Drawable drawable in textView.GetCompoundDrawables())
			{
				if (drawable != null)
					drawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor(color), PorterDuff.Mode.SrcIn));
			}
		}
	}
}