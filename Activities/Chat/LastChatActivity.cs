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
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.Chat.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Messages;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActionMode = AndroidX.AppCompat.View.ActionMode;
using Object = Java.Lang.Object;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Chat
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class LastChatActivity : BaseActivity, IOnClickListenerSelected
	{
		#region Variables Basic

		public static LastChatAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout;
		private View Inflated;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private static ActionMode ActionMode;
		private static Toolbar ToolBar;
		private AdView MAdView;
		private ActionModeCallback ModeCallback;
		private string UserId = "";
		private static LastChatActivity Instance;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.RecyclerDefaultLayout);

				Instance = this;

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				SetRecyclerViewAdapters();

				GetLastChatLocal();

				AdsGoogle.Ad_Interstitial(this);
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
				AdsGoogle.LifecycleAdView(MAdView, "Resume");
				base.OnResume();
				AddOrRemoveEvent(true);
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
				AdsGoogle.LifecycleAdView(MAdView, "Pause");
				base.OnPause();
				AddOrRemoveEvent(false);
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
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void OnTrimMemory(TrimMemory level)
		{
			try
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				base.OnTrimMemory(level);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		protected override void OnDestroy()
		{
			try
			{
				ListUtils.ChatList = MAdapter.UserList;

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
				SwipeRefreshLayout.Refreshing = false;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
				SwipeRefreshLayout.SetPadding(0, 0, 0, 0);

				MAdView = FindViewById<AdView>(Resource.Id.adView);
				MAdView.Visibility = ViewStates.Visible;
				AdsGoogle.InitAdView(MAdView, MRecycler);
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
				MAdapter = new LastChatAdapter(this);
				LayoutManager = new LinearLayoutManager(this);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<GetChatsObject.Data>(this, MAdapter, sizeProvider, 10);
				MRecycler.AddOnScrollListener(preLoader);
				MRecycler.SetAdapter(MAdapter);
				MAdapter.SetOnClickListener(this);

				ModeCallback = new ActionModeCallback(this);

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

		private void InitToolbar()
		{
			try
			{
				ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (ToolBar != null)
				{
					var toolbarTitle = FindViewById<TextView>(Resource.Id.toolbar_title);
					toolbarTitle.Text = GetText(Resource.String.Lbl_Chats);

					ToolBar.Title = " ";
					ToolBar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

					SetSupportActionBar(ToolBar);
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

		private void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				}
				else
				{
					SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static LastChatActivity GetInstance()
		{
			try
			{
				return Instance;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		#endregion

		#region Scroll

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
		{
			try
			{
				//Event Scroll #LastChat
				var item = MAdapter.UserList.LastOrDefault();
				if (item != null && MAdapter.UserList.Count > 10)
				{
					StartApiService(item.Id);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Toolbar & Selected

		private class ActionModeCallback : Object, ActionMode.ICallback
		{
			private readonly LastChatActivity Activity;
			public ActionModeCallback(LastChatActivity activity)
			{
				Activity = activity;
			}

			public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
			{
				int id = item.ItemId;
				switch (id)
				{
					case Resource.Id.action_delete:
						DeleteItems();
						mode.Finish();
						return true;
					case Android.Resource.Id.Home:
						TabbedMainActivity.GetInstance()?.SetService();

						MAdapter.ClearSelections();

						ActionMode.Finish();

						return true;
					default:
						return false;
				}
			}

			public bool OnCreateActionMode(ActionMode mode, IMenu menu)
			{
				SetSystemBarColor(Activity, AppSettings.MainColor);
				mode.MenuInflater?.Inflate(Resource.Menu.menu_delete, menu);
				return true;
			}

			private void SetSystemBarColor(Activity act, string color)
			{
				try
				{
					if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
					{
						Window window = act.Window;
						window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
						window.ClearFlags(WindowManagerFlags.TranslucentStatus);
						window.SetStatusBarColor(Color.ParseColor(color));
					}
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}

			public void OnDestroyActionMode(ActionMode mode)
			{
				try
				{
					MAdapter.ClearSelections();
					ActionMode = null;
					SetSystemBarColor(Activity, AppSettings.MainColor);

					TabbedMainActivity.GetInstance()?.SetService();

					ToolBar.Visibility = ViewStates.Visible;
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}

			public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
			{
				return false;
			}

			//Delete Chat 
			private void DeleteItems()
			{
				try
				{
					TabbedMainActivity.GetInstance()?.SetService();

					if (ToolBar.Visibility != ViewStates.Visible)
						ToolBar.Visibility = ViewStates.Visible;

					if (Methods.CheckConnectivity())
					{
						List<int> selectedItemPositions = MAdapter.GetSelectedItems();
						for (int i = selectedItemPositions.Count - 1; i >= 0; i--)
						{
							var datItem = MAdapter.GetItem(selectedItemPositions[i]);
							if (datItem != null)
							{
								MAdapter.RemoveData(selectedItemPositions[i], datItem);
								//Send Api Delete 
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Messages.DeleteChat(datItem.Id) });

							}
						}

						MAdapter.NotifyDataSetChanged();
					}
					else
					{
						Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}
		}

		//Event 
		public void OnItemClick(View view, GetChatsObject.Data obj, int pos)
		{
			try
			{
				if (MAdapter.GetSelectedItemCount() > 0) // Add Select  New Item 
				{
					EnableActionMode(pos);
				}
				else
				{
					TabbedMainActivity.GetInstance()?.SetService();

					if (ToolBar.Visibility != ViewStates.Visible)
						ToolBar.Visibility = ViewStates.Visible;

					// read the item which removes bold from the row >> event click open ChatBox by user id
					GetChatsObject.Data item = MAdapter.GetItem(pos);
					if (item != null)
					{
						UserId = item.User.Id;

						item.GetCountSeen = "0";
						Intent intent = new Intent(this, typeof(MessagesBoxActivity));
						intent.PutExtra("UserId", item.User.Id);
						intent.PutExtra("TypeChat", "LastChat");
						intent.PutExtra("UserItem", JsonConvert.SerializeObject(item));

						StartActivity(intent);
						MAdapter.NotifyItemChanged(pos);
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnItemLongClick(View view, GetChatsObject.Data obj, int pos)
		{
			EnableActionMode(pos);
		}

		private void EnableActionMode(int position)
		{
			if (ActionMode == null)
			{
				ActionMode = StartSupportActionMode(ModeCallback);
			}
			ToggleSelection(position);
		}

		private void ToggleSelection(int position)
		{
			try
			{
				MAdapter.ToggleSelection(position);
				int count = MAdapter.GetSelectedItemCount();

				if (count == 0)
				{
					TabbedMainActivity.GetInstance()?.SetService();

					ToolBar.Visibility = ViewStates.Visible;
					ActionMode.Finish();
				}
				else
				{
					TabbedMainActivity.GetInstance()?.SetService(false);

					ToolBar.Visibility = ViewStates.Gone;
					ActionMode.SetTitle(count);
					ActionMode.Invalidate();
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
				MAdapter.UserList.Clear();
				MAdapter.NotifyDataSetChanged();

				SqLiteDatabase database = new SqLiteDatabase();
				database.ClearLastChat();

				StartApiService();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		private void GetLastChatLocal()
		{
			try
			{
				SqLiteDatabase dbDatabase = new SqLiteDatabase();
				ListUtils.ChatList = new ObservableCollection<GetChatsObject.Data>();
				var list = dbDatabase.GetAllLastChat();
				if (list.Count > 0)
				{
					ListUtils.ChatList = new ObservableCollection<GetChatsObject.Data>(list);
					MAdapter.UserList = ListUtils.ChatList;
					MAdapter.BindEnd();
				}
				else
				{
					SwipeRefreshLayout.Refreshing = true;

					StartApiService();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (Methods.CheckConnectivity())
			{
				int countList = MAdapter.UserList.Count;

				var (apiStatus, respond) = await RequestsAsync.Messages.GetChats("10", offset);
				if (apiStatus != 200 || respond is not GetChatsObject result || result.data == null)
				{
					//Methods.DisplayReportResult(this, respond);
				}
				else
				{
					result.data.RemoveAll(a => a.Id == null || a.User == null);

					var respondList = result.data.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							LoadDataJsonLastChat(result);
						}
						else
						{
							ListUtils.ChatList = new ObservableCollection<GetChatsObject.Data>(result.data);
							MAdapter.UserList = new ObservableCollection<GetChatsObject.Data>(result.data);
							RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });

							SqLiteDatabase dbDatabase = new SqLiteDatabase();
							dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
						}
					}
					else
					{
						if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
					}
				}

				MainScrollEvent.IsLoading = false;
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

		public void LoadDataJsonLastChat(GetChatsObject result)
		{
			try
			{
				if (MAdapter != null)
				{
					if (MAdapter.UserList?.Count > 0)
					{
						foreach (var user in result.data)
						{
							var checkUser = MAdapter.UserList.FirstOrDefault(a => a.User.Id == user.User.Id);
							if (checkUser != null)
							{
								//checkUser.UserId = user.UserId;
								//checkUser.Id = user.Id;
								if (checkUser.UserOne != user.UserOne) checkUser.UserOne = user.UserOne;
								if (checkUser.UserTwo != user.UserTwo) checkUser.UserTwo = user.UserTwo;
								if (checkUser.Time != user.Time) checkUser.Time = user.Time;
								if (checkUser.TextTime != user.TextTime) checkUser.TextTime = user.TextTime;
								if (checkUser.User != user.User) checkUser.User = user.User;
								if (checkUser.GetCountSeen != user.GetCountSeen) checkUser.GetCountSeen = user.GetCountSeen;

								if (checkUser.GetLastMessage.Text == user.GetLastMessage.Text) continue;

								if (checkUser.GetLastMessage != user.GetLastMessage) checkUser.GetLastMessage = user.GetLastMessage;

								checkUser.GetLastMessage.Text = user.GetLastMessage.Text;
								var index = MAdapter.UserList.IndexOf(MAdapter.UserList.FirstOrDefault(a => a.User.Id == user.User.Id));
								if (index > -1)
								{
									RunOnUiThread(() =>
									{
										MAdapter.Move(checkUser);
									});
								}
							}
							else
							{
								RunOnUiThread(() =>
								{
									MAdapter.Insert(user);

									var dataUser = MAdapter.UserList.IndexOf(MAdapter.UserList.FirstOrDefault(a => a.Id == user.Id));
									if (dataUser > -1)
										MAdapter.NotifyItemChanged(dataUser);
								});
							}
						}
					}
					else
					{
						MAdapter.UserList = new ObservableCollection<GetChatsObject.Data>(result.data);
						MAdapter.BindEnd();
					}

					ListUtils.ChatList = MAdapter.UserList;
				}

				SqLiteDatabase dbDatabase = new SqLiteDatabase();
				dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void ShowEmptyPage()
		{
			try
			{
				SwipeRefreshLayout.Refreshing = false;

				if (MAdapter.UserList.Count > 0)
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
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoMessage);
					if (!x.EmptyStateButton.HasOnClickListeners)
					{
						x.EmptyStateButton.Click += null;
					}
					EmptyStateLayout.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
				SwipeRefreshLayout.Refreshing = false;
				Methods.DisplayReportResultTrack(e);
			}
		}

		//No Internet Connection 
		private void EmptyStateButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				StartApiService();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

	}
}