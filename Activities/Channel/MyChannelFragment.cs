using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using PlayTube.Activities.Channel.Tab;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Tabbes;
using PlayTube.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Channel
{
	public class MyChannelFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy
	{
		#region Variables Basic
		public static MyChannelFragment Instance;
		public ChPlayListFragment PlayListFragment;
		public ChVideosFragment VideosFragment;
		public ChShortsFragment ShortsFragment;
		public ChActivitiesFragment ActivitiesFragment;
		private AppBarLayout AppBarLayout;
		private TabLayout Tabs;
		private ViewPager2 ViewPagerView;
		private ImageView ImageChannel, ChannelVerifiedText, ImageCoverChannel, IconSettings;
		private CollapsingToolbarLayout CollapsingToolbar;
		private TextView ChannelNameText, SubcribersCountText;
		public TextView VideoCountText;
		private TabbedMainActivity MainContext;
		private AppCompatButton SubscribeChannelButton;
		private MainTabAdapter Adapter;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			MainContext = (TabbedMainActivity)Activity;
			HasOptionsMenu = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// Use this to return your custom view for this Fragment
				View view = inflater?.Inflate(Resource.Layout.MyChannelFragmentLayout, container, false);
				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				base.OnViewCreated(view, savedInstanceState);

				Instance = this;

				//Get Value And Set Toolbar
				InitComponent(view);
				InitToolbar(view);
				InitTab();
				GetDataChannelApi();

				SubscribeChannelButton.Click += SubscribeChannelButtonOnClick;
				IconSettings.Click += IconSettingsOnClick;

				AdsGoogle.Ad_Interstitial(MainContext);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void OnLowMemory()
		{
			try
			{
				GC.Collect(GC.MaxGeneration);
				base.OnLowMemory();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void OnDestroyView()
		{
			try
			{
				Instance = null;
				base.OnDestroyView();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				ImageCoverChannel = view.FindViewById<ImageView>(Resource.Id.myImagevideo);
				ImageChannel = view.FindViewById<ImageView>(Resource.Id.myChannelImage);
				CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.mycollapsingToolbar);
				ChannelNameText = view.FindViewById<TextView>(Resource.Id.myChannelName);

				ChannelVerifiedText = view.FindViewById<ImageView>(Resource.Id.myChannelVerifiedText);
				IconSettings = view.FindViewById<ImageView>(Resource.Id.mySettings_icon);
				SubscribeChannelButton = view.FindViewById<AppCompatButton>(Resource.Id.mySubcribeChannelButton);

				Tabs = view.FindViewById<TabLayout>(Resource.Id.mychanneltabs);
				ViewPagerView = view.FindViewById<ViewPager2>(Resource.Id.myChannelviewpager);
				AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.mymainAppBarLayout);
				AppBarLayout.SetExpanded(true);

				SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Edit);

				ChannelVerifiedText.Visibility = ViewStates.Gone;
				//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ChannelVerifiedText,IonIconsFonts.CheckmarkCircle);

				VideoCountText = view.FindViewById<TextView>(Resource.Id.videocount);
				SubcribersCountText = view.FindViewById<TextView>(Resource.Id.subcriberscount);

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar(View view)
		{
			try
			{
				var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
				MainContext.SetToolBar(toolbar, " ");

				var icon = AppCompatResources.GetDrawable(Context, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
				icon?.SetTint(Color.White);
				MainContext.SupportActionBar.SetHomeAsUpIndicator(icon);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitTab()
		{
			try
			{
				ViewPagerView.OffscreenPageLimit = 3;
				SetUpViewPager(ViewPagerView);
				//Tabs.SetTabTextColors(Color.White, Color.ParseColor(AppSettings.MainColor));
				new TabLayoutMediator(Tabs, ViewPagerView, this).Attach();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Tab

		private void SetUpViewPager(ViewPager2 viewPager)
		{
			try
			{
				PlayListFragment = new ChPlayListFragment();
				VideosFragment = new ChVideosFragment();
				ShortsFragment = new ChShortsFragment();
				ChAboutFragment aboutFragment = new ChAboutFragment();
				ActivitiesFragment = new ChActivitiesFragment();

				Bundle bundle = new Bundle();
				bundle.PutString("ChannelId", UserDetails.UserId);

				PlayListFragment.Arguments = bundle;
				VideosFragment.Arguments = bundle;
				ShortsFragment.Arguments = bundle;
				ActivitiesFragment.Arguments = bundle;
				aboutFragment.Arguments = bundle;

				Adapter = new MainTabAdapter(this);
				Adapter.AddFragment(VideosFragment, GetText(Resource.String.Lbl_Videos));
				Adapter.AddFragment(ShortsFragment, GetText(Resource.String.Lbl_Shorts));

				if (AppSettings.AllowPlayLists)
					Adapter.AddFragment(PlayListFragment, GetText(Resource.String.Lbl_PlayLists));

				if (AppSettings.ShowActivities)
					Adapter.AddFragment(ActivitiesFragment, GetText(Resource.String.Lbl_Activities));

				Adapter.AddFragment(aboutFragment, GetText(Resource.String.Lbl_AboutChannal));

				viewPager.Orientation = ViewPager2.OrientationHorizontal;
				viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
				viewPager.Adapter = Adapter;
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
			private readonly MyChannelFragment Fragment;

			public MyOnPageChangeCallback(MyChannelFragment fragment)
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
					var p = position;
					switch (p)
					{
						case 0:
							break;
						case 1:
							break;
						case 2:
							if (AppSettings.AllowPlayLists)
								Task.Factory.StartNew(() => Fragment?.PlayListFragment?.StartApiService());
							break;
						case 3:
							break;
					}
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}
		}

		#endregion

		#region Get Data Channel

		public async void GetDataChannelApi()
		{
			try
			{
				SetDataUser();

				var data = await ApiRequest.GetChannelData(Activity, UserDetails.UserId);
				if (data == null) return;
				SetDataUser();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void SetDataUser()
		{
			try
			{
				var dataChannel = ListUtils.MyChannelList?.FirstOrDefault();
				if (dataChannel != null)
				{
					var name = AppTools.GetNameFinal(dataChannel);

					CollapsingToolbar.Title = name;
					ChannelNameText.Text = name;

					GlideImageLoader.LoadImage(Activity, dataChannel.Avatar, ImageChannel, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
					Glide.With(Activity).Load(dataChannel.Cover).Apply(new RequestOptions().FitCenter()).Into(ImageCoverChannel);

					if (dataChannel.Verified == "1")
						ChannelVerifiedText.Visibility = ViewStates.Visible;

					SubcribersCountText.Text = dataChannel.SubscribeCount;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Events

		private void SubscribeChannelButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(Activity, typeof(EditMyChannelActivity));
				Activity.StartActivityForResult(intent, 252);

				MainContext.VideoDataWithEventsLoader?.OnStop();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void IconSettingsOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(Activity, typeof(SettingsActivity));
				Activity.StartActivity(intent);
				MainContext.VideoDataWithEventsLoader?.OnStop();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
		#endregion

	}
}