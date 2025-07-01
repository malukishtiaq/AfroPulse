using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using PlayTube.Activities.Base;
using PlayTube.Activities.ChannelPopular.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.ChannelPopular
{
	public class AllChannelPopularFragment : RecyclerViewDefaultBaseFragment
	{
		#region Variables Basic

		public PopChannelRowAdapter MAdapter;
		public SwipeRefreshLayout SwipeRefreshLayout;
		public RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		public ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private TabbedMainActivity GlobalContext;
		private FrameLayout FilterButton;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			GlobalContext = (TabbedMainActivity)Activity;
			HasOptionsMenu = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// Use this to return your custom view for this Fragment
				View view = inflater?.Inflate(Resource.Layout.RecyclerDefaultLayout, container, false);
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
				//Get Value And Set Toolbar
				InitComponent(view);
				InitShimmer(view);
				InitToolbar(view);
				SetRecyclerViewAdapters();

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;
				MAdapter.SubscribeButtonClick += MAdapterOnSubscribeButtonClick;

				//Get Data Api
				Task.Factory.StartNew(() => StartApiService());

				AdsGoogle.Ad_RewardedInterstitial(Activity);
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

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
				EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

				SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

				FilterButton = view.FindViewById<FrameLayout>(Resource.Id.toolbar_more);
				FilterButton.Visibility = ViewStates.Visible;
				FilterButton.Click += FilterButtonOnClick;

				ShowGoogleAds(view, MRecycler);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitShimmer(View view)
		{
			try
			{
				ShimmerPageLayout = view.FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
				InflatedShimmer ??= ShimmerPageLayout.Inflate();

				ShimmerInflater = new TemplateShimmerInflater();
				ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.ChannelTemplate);
				ShimmerInflater.Show();
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
				MAdapter = new PopChannelRowAdapter(Activity)
				{
					ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>()
				};

				LayoutManager = new LinearLayoutManager(Context);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<GetPopularChannelsObject.Channel>(Activity, MAdapter, sizeProvider, 10);
				MRecycler.AddOnScrollListener(preLoader);
				MRecycler.SetAdapter(MAdapter);

				RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
				MainScrollEvent = xamarinRecyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
				MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;
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
				var toolbarTitle = view.FindViewById<TextView>(Resource.Id.toolbar_title);
				GlobalContext.SetToolBar(toolbar, GetText(Resource.String.Lbl_PopularChannels), toolbarTitle);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void MAdapterOnSubscribeButtonClick(object sender, PopChannelRowAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						if (!string.IsNullOrEmpty(item.UserData?.SubscriberPrice) && item.UserData?.SubscriberPrice != "0")
						{
							if (e.SubscribeButton.Tag?.ToString() == "PaidSubscribe")
							{
								//This channel is paid, You must pay to subscribe
								GlobalContext.OpenDialog(item.UserData, Convert.ToDouble(item.UserData?.SubscriberPrice));
							}
							else
							{
								e.SubscribeButton.Tag = "Subscribe";
								e.SubscribeButton.Text = Context.GetText(Resource.String.Lbl_Subscribe);

								// background
								e.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_normal);

								//Color
								e.SubscribeButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
								//icon
								//Drawable icon = Context.GetDrawable(Resource.Drawable.SubcribeButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//e.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(item.UserData?.Id);


								//Send API Request here for UnSubcribe 
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(item.UserData?.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
						else
						{
							if (e.SubscribeButton.Tag?.ToString() == "Subscribe")
							{
								e.SubscribeButton.Tag = "Subscribed";
								e.SubscribeButton.Text = Context.GetText(Resource.String.Lbl_Subscribed);

								// background
								e.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_pressed);

								//Color
								e.SubscribeButton.SetTextColor(Color.White);
								//icon
								//Drawable icon = Context.GetDrawable(Resource.Drawable.SubcribedButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//e.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Add The Video to  Subcribed Videos Database
								Events_Insert_SubscriptionsChannel(item.UserData);

								//Send API Request here for Subcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(item.UserData?.Id) });


								Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
							}
							else
							{
								e.SubscribeButton.Tag = "Subscribe";
								e.SubscribeButton.Text = Context.GetText(Resource.String.Lbl_Subscribe);

								// background
								e.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_normal);

								//Color
								e.SubscribeButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
								//icon
								//Drawable icon = Context.GetDrawable(Resource.Drawable.SubcribeButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//e.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(item.UserData?.Id);


								//Send API Request here for UnSubcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(item.UserData?.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
						dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Warning), GlobalContext.GetText(Resource.String.Lbl_Please_sign_in_Subcribed), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void Events_Insert_SubscriptionsChannel(UserDataObject owner)
		{
			try
			{
				var sqlEntity = new SqLiteDatabase();

				if (owner != null)
					sqlEntity.Insert_One_SubscriptionChannel(owner);

				GlobalContext.LibrarySynchronizer.AddToSubscriptions(owner);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void MAdapterOnItemClick(object sender, PopChannelRowAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.ShowUserChannelFragment(item.UserData, item.UserData.Id);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
		{
			try
			{
				ShimmerInflater?.Show();

				//Get Data Api
				MAdapter.ChannelList.Clear();
				MAdapter.NotifyDataSetChanged();

				MainScrollEvent.IsLoading = false;
				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void FilterButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var searchFilter = new ChannelPopularFilterBottomDialogFragment();
				searchFilter.Show(Activity.SupportFragmentManager, searchFilter.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		#endregion

		#region Scroll

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
		{
			try
			{
				//Code get last id where LoadMore >> 
				var lastItem = MAdapter.ChannelList.LastOrDefault();
				if (lastItem != null && !MainScrollEvent.IsLoading)
				{
					string totalId = lastItem.UserData.Id;
					var all = MAdapter.ChannelList.Where(a => a.Views == lastItem.Views).ToList();
					if (all.Count > 1)
					{
						//Get all id 
						totalId = all.Aggregate(totalId, (current, item) => current + item.UserData.Id + ",");
						totalId = totalId.Remove(totalId.Length - 1, 1);
					}

					StartApiService(totalId, lastItem.Count);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		public void StartApiService(string offset = "0", string lastCount = "")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset, lastCount) });
		}

		private async Task LoadDataAsync(string offset = "0", string lastCount = "")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;
				int countList = MAdapter.ChannelList.Count;

				var (apiStatus, respond) = await RequestsAsync.Video.PopularChannelsAsync("10", lastCount, offset, UserDetails.FilterTypeSortBy, UserDetails.FilterTime);
				if (apiStatus != 200 || respond is not GetPopularChannelsObject result || result.Channels == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.Channels.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							foreach (var item in result.Channels)
							{
								var check = MAdapter.ChannelList.FirstOrDefault(a => a.UserData?.Id == item.UserData?.Id);
								if (check == null)
								{
									MAdapter.ChannelList.Add(item);
								}
							}
							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.ChannelList.Count - countList); });
						}
						else
						{
							MAdapter.ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>(result.Channels);
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.ChannelList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreChannels), ToastLength.Short)?.Show();
					}
				}

				Activity?.RunOnUiThread(ShowEmptyPage);
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

				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		private void ShowEmptyPage()
		{
			try
			{
				ShimmerInflater?.Hide();
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;

				if (MAdapter.ChannelList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}
				else
				{
					MRecycler.Visibility = ViewStates.Gone;

					if (Inflated == null)
						Inflated = EmptyStateLayout?.Inflate();

					EmptyStateInflater x = new EmptyStateInflater();
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoChannels);
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
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;
				Methods.DisplayReportResultTrack(e);
			}
		}

		//No Internet Connection 
		private void EmptyStateButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
		#endregion
	}
}