using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Google.Android.Gms.Ads;
using Google.Android.Material.Dialog;
using PlayTube.Activities.Base;
using PlayTube.Activities.SettingsPreferences.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences.Security
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class ManageSessionsActivity : BaseActivity
	{
		#region Variables Basic

		private ManageSessionsAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private AdView MAdView;
		private FetchSessionsObject.SessionsDataObject ItemSessionsDataObject;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				Methods.App.FullScreenApp(this);

				// Create your application here
				SetContentView(Resource.Layout.RecyclerDefaultLayout);

				//Get Value And Set Toolbar
				InitComponent();
				InitShimmer();
				InitToolbar();
				SetRecyclerViewAdapters();

				Task.Factory.StartNew(StartApiService);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnResume()
		{
			try
			{
				base.OnResume();
				AddOrRemoveEvent(true);
				AdsGoogle.LifecycleAdView(MAdView, "Resume");
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnPause()
		{
			try
			{
				base.OnPause();
				AddOrRemoveEvent(false);
				AdsGoogle.LifecycleAdView(MAdView, "Pause");
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnTrimMemory(TrimMemory level)
		{
			try
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				base.OnTrimMemory(level);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
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

		protected override void OnDestroy()
		{
			try
			{
				AdsGoogle.LifecycleAdView(MAdView, "Destroy");
				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Menu 

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		#endregion

		#region Functions

		private void InitComponent()
		{
			try
			{
				MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
				EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

				SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
				SwipeRefreshLayout.SetPadding(0, 0, 0, 0);

				MAdView = FindViewById<AdView>(Resource.Id.adView);
				AdsGoogle.InitAdView(MAdView, MRecycler);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitShimmer()
		{
			try
			{
				ShimmerPageLayout = FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
				InflatedShimmer ??= ShimmerPageLayout.Inflate();

				ShimmerInflater = new TemplateShimmerInflater();
				ShimmerInflater.InflateLayout(this, InflatedShimmer, ShimmerTemplateStyle.VideoRowTemplate);
				ShimmerInflater.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar()
		{
			try
			{
				var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					var toolbarTitle = FindViewById<TextView>(Resource.Id.toolbar_title);
					toolbarTitle.Visibility = ViewStates.Visible;
					toolbarTitle.Text = GetString(Resource.String.Lbl_ManageSessions);

					toolbar.Title = " ";

					toolbar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

					SetSupportActionBar(toolbar);
					SupportActionBar.SetDisplayShowCustomEnabled(true);
					SupportActionBar.SetDisplayHomeAsUpEnabled(true);
					SupportActionBar.SetHomeButtonEnabled(true);
					SupportActionBar.SetDisplayShowHomeEnabled(true);

					var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
					icon?.SetTint(AppTools.IsTabDark() ? Color.White : Color.Black);
					SupportActionBar.SetHomeAsUpIndicator(icon);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetRecyclerViewAdapters()
		{
			try
			{
				MAdapter = new ManageSessionsAdapter(this)
				{
					SessionsList = new ObservableCollection<FetchSessionsObject.SessionsDataObject>()
				};
				LayoutManager = new LinearLayoutManager(this);
				MRecycler.SetLayoutManager(LayoutManager);
				//MRecycler.HasFixedSize = true;
				//MRecycler.SetItemViewCacheSize(10);
				//MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				MRecycler.SetAdapter(MAdapter);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					MAdapter.CloseItemClick += MAdapterOnItemClick;
					SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				}
				else
				{
					MAdapter.CloseItemClick -= MAdapterOnItemClick;
					SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		//Refresh
		private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
		{
			try
			{
				ShimmerInflater?.Show();

				MAdapter.SessionsList.Clear();
				MAdapter.NotifyDataSetChanged();

				Task.Factory.StartNew(StartApiService);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MAdapterOnItemClick(object sender, ManageSessionsAdapterClickEventArgs e)
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
					return;
				}

				ItemSessionsDataObject = MAdapter.GetItem(e.Position);
				if (ItemSessionsDataObject != null)
				{
					var dialog = new MaterialAlertDialogBuilder(this);

					dialog.SetTitle(Resource.String.Lbl_Warning);
					dialog.SetMessage(GetText(Resource.String.Lbl_AreYouSureLogoutFromThisDevice));
					dialog.SetPositiveButton(GetText(Resource.String.Lbl_Ok), (o, args) =>
					{
						try
						{
							if (ItemSessionsDataObject == null) return;
							var index = MAdapter.SessionsList.IndexOf(MAdapter.SessionsList.FirstOrDefault(a => a.Id == ItemSessionsDataObject.Id));
							if (index == -1) return;
							MAdapter.SessionsList.Remove(ItemSessionsDataObject);
							MAdapter.NotifyItemRemoved(index);

							if (Methods.CheckConnectivity())
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.DeleteSessionsAsync(ItemSessionsDataObject.Id) });
							else
								Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
						}
					});
					dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

					dialog.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Sessions 

		private void StartApiService()
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadSessionsAsync });
		}

		private async Task LoadSessionsAsync()
		{
			if (Methods.CheckConnectivity())
			{
				int countList = MAdapter.SessionsList.Count;
				var (apiStatus, respond) = await RequestsAsync.Global.GetSessionsAsync();
				if (apiStatus != 200 || respond is not FetchSessionsObject result || result.Data == null)
				{
					Methods.DisplayReportResult(this, respond);
				}
				else
				{
					var respondList = result.Data.Count;
					if (respondList > 0)
					{
						foreach (var item in from item in result.Data let check = MAdapter.SessionsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
						{
							if (item.Browser != null)
								MAdapter.SessionsList.Add(item);
						}

						if (countList > 0)
						{
							RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SessionsList.Count - countList); });
						}
						else
						{
							RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
				}

				RunOnUiThread(ShowEmptyPage);
			}
			else
			{
				Inflated = EmptyStateLayout?.Inflate();
				EmptyStateInflater x = new EmptyStateInflater();
				x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
				if (!x.EmptyStateButton.HasOnClickListeners)
				{
					x.EmptyStateButton.Click += null;
					x.EmptyStateButton.Click += EmptyStateButtonOnClick;
				}

				Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
		}

		private void ShowEmptyPage()
		{
			try
			{
				ShimmerInflater?.Hide();
				SwipeRefreshLayout.Refreshing = false;

				if (MAdapter.SessionsList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}
				else
				{
					MRecycler.Visibility = ViewStates.Gone;

					Inflated ??= EmptyStateLayout?.Inflate();

					EmptyStateInflater x = new EmptyStateInflater();
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoSessions);
					if (!x.EmptyStateButton.HasOnClickListeners)
					{
						x.EmptyStateButton.Click += null;
					}
					EmptyStateLayout.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
				ShimmerInflater?.Hide();
				SwipeRefreshLayout.Refreshing = false;
				Methods.DisplayReportResultTrack(e);
			}
		}

		//No Internet Connection 
		private void EmptyStateButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				Task.Factory.StartNew(StartApiService);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

	}
}