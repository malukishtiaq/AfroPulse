using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using PlayTube.Activities.ChannelActivities;
using PlayTube.Activities.ChannelActivities.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Activities;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PlayTube.Activities.Channel.Tab
{
	public class ChActivitiesFragment : Fragment
	{
		#region Variables Basic

		private ActivitiesAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private TabbedMainActivity GlobalContext;
		private string IdChannel = "";
		public ActivitiesViewFragment ActivitiesViewFragment;

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
				View view = inflater?.Inflate(Resource.Layout.MainFragmentLayout, container, false);
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
				IdChannel = Arguments?.GetString("ChannelId");

				//Get Value And Set Toolbar
				InitComponent(view);
				InitShimmer(view);
				SetRecyclerViewAdapters();

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;
				MAdapter.LikeItemClick += MAdapterOnLikeItemClick;
				MAdapter.ShareItemClick += MAdapterOnItemClick; //wael add share after update api when add url 

				//Get Data Api
				Task.Factory.StartNew(() => StartApiService());
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
				ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.VideoRowTemplate);
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
				MAdapter = new ActivitiesAdapter(Activity)
				{
					ActivitiesList = new ObservableCollection<ActivitiesDataObject>()
				};
				LayoutManager = new LinearLayoutManager(Context);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<VideoDataObject>(Activity, MAdapter, sizeProvider, 10);
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

		#endregion

		#region Events

		private void MAdapterOnItemClick(object sender, ActivitiesAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				Bundle bundle = new Bundle();
				bundle.PutString("DataItemActivity", JsonConvert.SerializeObject(item));
				bundle.PutString("ActivityId", item.Id);

				ActivitiesViewFragment = new ActivitiesViewFragment { Arguments = bundle };
				GlobalContext.FragmentBottomNavigator.DisplayFragment(ActivitiesViewFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//DisLike
		//private void MAdapterOnDisLikeItemClick(object sender, ActivitiesAdapterClickEventArgs e)
		//{
		//    try
		//    {
		//        if (Methods.CheckConnectivity())
		//        {
		//            if (UserDetails.IsLogin)
		//            {
		//                if (e.Position <= -1) return;

		//                var item = MAdapter.GetItem(e.Position);
		//                if (item == null) return;

		//                item.IsDisliked = item.IsDisliked switch
		//                {
		//                    "0" => "1",
		//                    "1" => "0",
		//                    _ => "1"
		//                };

		//                switch (item.IsDisliked)
		//                {
		//                    case "1":
		//                        {
		//                            if (!item.Dislikes.Contains("K") && !item.Dislikes.Contains("M"))
		//                            {
		//                                var x = Convert.ToDouble(item.Dislikes);
		//                                x++;
		//                                item.Dislikes = x.ToString(CultureInfo.CurrentCulture);
		//                            }
		//                            break;
		//                        }
		//                    case "0":
		//                        {
		//                            if (!item.Dislikes.Contains("K") && !item.Dislikes.Contains("M"))
		//                            {
		//                                var x = Convert.ToDouble(item.Dislikes);
		//                                if (x > 0)
		//                                    x--;
		//                                else
		//                                    x = 0;

		//                                item.Dislikes = x.ToString(CultureInfo.CurrentCulture);
		//                            }
		//                            break;
		//                        }
		//                }

		//                if (item.IsLiked == "1")
		//                {
		//                    item.IsLiked = "0";

		//                    if (!item.Likes.Contains("K") && !item.Likes.Contains("M"))
		//                    {
		//                        var x = Convert.ToDouble(item.Likes);
		//                        if (x > 0)
		//                            x--;
		//                        else
		//                            x = 0;

		//                        item.Likes = x.ToString(CultureInfo.CurrentCulture);
		//                    }
		//                }

		//                MAdapter.NotifyItemChanged(e.Position);

		//                if (!Methods.CheckConnectivity())
		//                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
		//                else
		//                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Activities.DisLikeActivityAsync(item.Id) });
		//            }
		//            else
		//            {
		//                PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
		//                dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Please_sign_in_Like), Context.GetText(Resource.String.Lbl_Yes), Context.GetText(Resource.String.Lbl_No));
		//            }
		//        }
		//        else
		//        {
		//            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
		//        } 
		//    }
		//    catch (Exception exception)
		//    {
		//        Methods.DisplayReportResultTrack(exception);
		//    }
		//}

		//Like
		private void MAdapterOnLikeItemClick(object sender, ActivitiesAdapterClickEventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{

						if (e.Position <= -1) return;

						var item = MAdapter.GetItem(e.Position);
						if (item == null) return;

						item.IsLiked = item.IsLiked switch
						{
							"0" => "1",
							"1" => "0",
							_ => "1"
						};

						switch (item.IsLiked)
						{
							case "1":
								{
									if (!item.Likes.Contains("K") && !item.Likes.Contains("M"))
									{
										var x = Convert.ToDouble(item.Likes);
										x++;
										item.Likes = x.ToString(CultureInfo.CurrentCulture);
									}
									break;
								}
							case "0":
								{
									if (!item.Likes.Contains("K") && !item.Likes.Contains("M"))
									{
										var x = Convert.ToDouble(item.Likes);
										if (x > 0)
											x--;
										else
											x = 0;

										item.Likes = x.ToString(CultureInfo.CurrentCulture);
									}
									break;
								}
						}

						if (item.IsDisliked == "1")
						{
							item.IsDisliked = "0";

							if (!item.Dislikes.Contains("K") && !item.Dislikes.Contains("M"))
							{
								var x = Convert.ToDouble(item.Dislikes);
								if (x > 0)
									x--;
								else
									x = 0;

								item.Dislikes = x.ToString(CultureInfo.CurrentCulture);
							}
						}

						MAdapter.NotifyItemChanged(e.Position);

						if (!Methods.CheckConnectivity())
							Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						else
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Activities.LikeActivityAsync(item.Id) });
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
						dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Please_sign_in_Dislike), Context.GetText(Resource.String.Lbl_Yes), Context.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
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
				MAdapter.ActivitiesList.Clear();
				MAdapter.NotifyDataSetChanged();

				MainScrollEvent.IsLoading = false;
				Task.Factory.StartNew(() => StartApiService());
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
				var item = MAdapter.ActivitiesList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
					StartApiService(item.Id);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		private void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;
				int countList = MAdapter.ActivitiesList.Count;

				var (apiStatus, respond) = await RequestsAsync.Activities.GetActivitiesAsync(IdChannel, "10", offset);
				if (apiStatus != 200 || respond is not GetActivitiesObject result || result.Data == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.Data.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							foreach (var item in from item in result.Data let check = MAdapter.ActivitiesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
							{
								MAdapter.ActivitiesList.Add(item);
							}

							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.ActivitiesList.Count - countList); });
						}
						else
						{
							MAdapter.ActivitiesList = new ObservableCollection<ActivitiesDataObject>(result.Data);
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.ActivitiesList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
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
				if (GlobalContext.UserChannelFragment != null && GlobalContext.UserChannelFragment.SubscribeChannelButton.Tag?.ToString() == "PaidSubscribe" && IdChannel != UserDetails.UserId)
				{
					Inflated ??= EmptyStateLayout?.Inflate();
					GlobalContext.UserChannelFragment.SetEmptyPageSubscribeChannelWithPaid(EmptyStateLayout, Inflated);
				}
				else
				{
					if (MAdapter.ActivitiesList.Count > 0)
					{
						MRecycler.Visibility = ViewStates.Visible;
						EmptyStateLayout.Visibility = ViewStates.Gone;
					}
					else
					{
						MRecycler.Visibility = ViewStates.Gone;

						Inflated ??= EmptyStateLayout?.Inflate();

						EmptyStateInflater x = new EmptyStateInflater();
						x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoActivities);
						if (!x.EmptyStateButton.HasOnClickListeners)
						{
							x.EmptyStateButton.Click += null;
						}
						EmptyStateLayout.Visibility = ViewStates.Visible;
					}
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