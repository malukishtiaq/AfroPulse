using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.Tabs;
using PlayTube.Activities.Default;
using PlayTube.Activities.Tabbes.HomePages;
using PlayTube.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlayTube.Activities.Tabbes.Fragments
{
	public class HomeFragment : AndroidX.Fragment.App.Fragment, TabLayoutMediator.ITabConfigurationStrategy
	{
		#region  Variables Basic

		private ImageView AppLogo;
		public ImageView ProfileButton, NotificationButton, SearchButton, ChatButton;

		private TabbedMainActivity ContextGlobal;

		private ViewPager2 ViewPager;
		private TabLayout TabLayout;

		public LatestHomeFragment LatestHomeTab;
		private MainTabAdapter Adapter;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				ContextGlobal = (TabbedMainActivity)Activity;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				View view = inflater.Inflate(Resource.Layout.THomeLayout, container, false);
				InitComponent(view);

				Task.Factory.StartNew(() =>
				{
					if (UserDetails.IsLogin)
						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetChannelData(Activity, UserDetails.UserId), () => ApiRequest.PlayListsVideosApi(Activity), () => ApiRequest.AdsVideosApi(Activity) });
				});

				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		public override void OnLowMemory()
		{
			try
			{
				GC.Collect(GC.MaxGeneration);
				base.OnLowMemory();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnDestroy()
		{
			try
			{
				base.OnDestroy();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				AppLogo = (ImageView)view.FindViewById(Resource.Id.app_logo);
				AppLogo.SetImageResource(AppTools.IsTabDark() ? Resource.Drawable.logolight : Resource.Drawable.logo);

				ViewPager = view.FindViewById<ViewPager2>(Resource.Id.ViewPager);
				TabLayout = view.FindViewById<TabLayout>(Resource.Id.tab_home);

				SetUpViewPager(ViewPager);

				ChatButton = (ImageView)view.FindViewById(Resource.Id.chatButton);
				ChatButton.Click += ContextGlobal.ChatButtonOnClick;

				if (!UserDetails.IsLogin)
					ChatButton.Visibility = ViewStates.Gone;

				SearchButton = (ImageView)view.FindViewById(Resource.Id.searchButton);
				SearchButton.Click += ContextGlobal.SearchButtonOnClick;

				NotificationButton = (ImageView)view.FindViewById(Resource.Id.notificationButton);
				NotificationButton.Click += ContextGlobal.NotificationButtonOnClick;

				ProfileButton = (ImageView)view.FindViewById(Resource.Id.profileButton);
				ProfileButton.Click += ContextGlobal.ProfileButtonOnClick;
				GlideImageLoader.LoadImage(Activity, UserDetails.Avatar, ProfileButton, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

				if (!UserDetails.IsLogin)
				{
					NotificationButton.Visibility = ViewStates.Gone;
					ProfileButton.Visibility = ViewStates.Gone;
				}

				LinearLayout mainAlert = (LinearLayout)view.FindViewById(Resource.Id.mainAlert);
				if (UserDetails.IsLogin)
				{
					mainAlert.Visibility = ViewStates.Gone;
				}
				else
				{
					ImageView headImage = (ImageView)view.FindViewById(Resource.Id.HeadImage);
					Glide.With(this).Load(Resource.Drawable.login).Apply(new RequestOptions()).Into(headImage);

					mainAlert.Visibility = ViewStates.Visible;
					mainAlert.Click += MainAlertOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private async void SetUpViewPager(ViewPager2 viewPager)
		{
			try
			{
				LatestHomeTab = new LatestHomeFragment();

				Adapter = new MainTabAdapter(this);

				Adapter.AddFragment(LatestHomeTab, GetText(Resource.String.Lbl_ForYou));

				if (CategoriesController.ListCategories.Count == 0)
				{
					var dbDatabase = new SqLiteDatabase();
					var list = dbDatabase.Get_Settings();
					if (list == null || list.Categories.Count == 0)
						await ApiRequest.GetSettings_Api(Activity);
				}

				var respondList = CategoriesController.ListCategories.Count;
				if (respondList > 0)
				{
					foreach (var cat in CategoriesController.ListCategories)
					{
						Bundle bundle = new Bundle();

						bundle.PutString("CategoryId", cat.Id);
						bundle.PutString("CategoryName", cat.Name);

						VideoCategoriesFragment fragment = new VideoCategoriesFragment { Arguments = bundle };
						Adapter.AddFragment(fragment, cat.Name);
					}
				}

				viewPager.UserInputEnabled = false;
				viewPager.CurrentItem = Adapter.ItemCount;
				viewPager.OffscreenPageLimit = Adapter.ItemCount;
				viewPager.ClearAnimation();

				viewPager.Orientation = ViewPager2.OrientationHorizontal;
				viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
				viewPager.Adapter = Adapter;
				viewPager.Adapter.NotifyDataSetChanged();

				new TabLayoutMediator(TabLayout, ViewPager, this).Attach();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		public void OnConfigureTab(TabLayout.Tab tab, int position)
		{
			try
			{
				tab.SetText(Adapter.GetFragment(position));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
		{
			private readonly HomeFragment Fragment;

			public MyOnPageChangeCallback(HomeFragment fragment)
			{
				try
				{
					Fragment = fragment;
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}

			public override void OnPageSelected(int position)
			{
				try
				{
					base.OnPageSelected(position);
					if (position % 3 == 0)
					{
						AdsGoogle.Ad_Interstitial(Fragment.Activity);
					}
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}
		}

		#endregion

		#region Event

		private void MainAlertOnClick(object sender, EventArgs e)
		{
			try
			{
				Activity.StartActivity(new Intent(Activity, typeof(LoginActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion
	}
}