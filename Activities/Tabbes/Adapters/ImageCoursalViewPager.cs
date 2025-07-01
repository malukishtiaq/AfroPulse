using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.ObjectModel;
using Object = Java.Lang.Object;

namespace PlayTube.Activities.Tabbes.Adapters
{
	public class ImageCoursalViewPager : PagerAdapter
	{
		private readonly Activity Context;
		private readonly ObservableCollection<VideoDataObject> VideoList;
		private readonly LayoutInflater Inflater;

		public ImageCoursalViewPager(Activity context, ObservableCollection<VideoDataObject> videoList)
		{
			Context = context;
			VideoList = videoList;
			Inflater = LayoutInflater.From(Context);
		}

		public override Object InstantiateItem(ViewGroup view, int position)
		{
			try
			{
				View layout = Inflater?.Inflate(Resource.Layout.ImageCoursalLayout, view, false);
				if (layout != null)
				{
					var mainFeaturedVideo = layout.FindViewById<ImageView>(Resource.Id.Imagevideo);
					var featuredVideoTitle = layout.FindViewById<TextView>(Resource.Id.TitleFeaturedVideo);
					var txtViewsCount = layout.FindViewById<TextView>(Resource.Id.Views_Count);

					var item = VideoList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(Context, item.Thumbnail, mainFeaturedVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

						featuredVideoTitle.Text = Methods.FunString.DecodeString(item.Title);
						txtViewsCount.Text = CategoriesController.GetCategoryName(item) + " | " + Methods.FunString.FormatPriceValue(Convert.ToInt32(item.Views)) + " " + Context.GetText(Resource.String.Lbl_Views) + " | " + item.TimeAgo;

						mainFeaturedVideo.Click += (sender, args) =>
						{
							TabbedMainActivity.GetInstance()?.StartPlayVideo(item);
						};

						featuredVideoTitle.Click += (sender, args) =>
						{
							TabbedMainActivity.GetInstance()?.StartPlayVideo(item);
						};
					}
					view.AddView(layout);
				}

				return layout;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}

		}

		public override bool IsViewFromObject(View view, Object @object)
		{
			return view.Equals(@object);
		}

		public override int Count
		{
			get
			{
				if (VideoList != null)
				{
					return VideoList.Count;
				}
				else
				{
					return 0;
				}
			}
		}

		public override void DestroyItem(ViewGroup container, int position, Object @object)
		{
			try
			{
				View view = (View)@object;
				container.RemoveView(view);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);

			}
		}
	}
}