using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Google.Android.Flexbox;
using Google.Android.Material.Dialog;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Search
{
	public class SearchFragment : Fragment, TextView.IOnEditorActionListener, IDialogListCallBack
	{
		#region Variables Basic

		private VideoBigAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private TabbedMainActivity GlobalContext;
		private string SearchKey = "", DateId = "";
		private TextView FilterButton, CloseButton;
		private FlexboxLayout CatLayout;
		private AutoCompleteTextView SearchView;

		private LinearLayout EmptyStateLayout;
		private AppCompatButton EmptyStateButton;
		private ImageView EmptyStateIcon;
		private TextView DescriptionText;
		private TextView TitleText;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			GlobalContext = TabbedMainActivity.GetInstance();
			HasOptionsMenu = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// Use this to return your custom view for this Fragment
				View view = inflater?.Inflate(Resource.Layout.SearchLayout, container, false);
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
				InitToolbar(view);
				SetRecyclerViewAdapters();
				CreateCategoryButtons();

				MRecycler.Visibility = ViewStates.Gone;
				EmptyStateLayout.Visibility = ViewStates.Visible;

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;
				FilterButton.Click += FilterButtonOnClick;
				CloseButton.Click += CloseButtonOnClick;

				AdsGoogle.Ad_RewardedVideo(Activity);
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
				FilterButton = (TextView)view.FindViewById(Resource.Id.IconFilter);
				CloseButton = (TextView)view.FindViewById(Resource.Id.IconClose);

				SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = false;
				SwipeRefreshLayout.Enabled = false;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

				MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

				EmptyStateLayout = (LinearLayout)view.FindViewById(Resource.Id.catLayout);
				EmptyStateIcon = (ImageView)view.FindViewById(Resource.Id.emtyicon);
				TitleText = (TextView)view.FindViewById(Resource.Id.headText);
				DescriptionText = (TextView)view.FindViewById(Resource.Id.seconderyText);
				EmptyStateButton = (AppCompatButton)view.FindViewById(Resource.Id.button);

				//EmptyStateIcon.SetImageResource(Resource.Drawable.icon_no_search_vector);
				//TitleText.Text = Context.GetText(Resource.String.Lbl_Empty_search);
				//DescriptionText.Text = Context.GetText(Resource.String.Lbl_Start_search);

				EmptyStateIcon.Visibility = ViewStates.Gone;
				TitleText.Visibility = ViewStates.Gone;
				DescriptionText.Visibility = ViewStates.Gone;
				EmptyStateButton.Visibility = ViewStates.Gone;

				CatLayout = (FlexboxLayout)view.FindViewById(Resource.Id.catFlexboxLayout);

				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FilterButton, IonIconsFonts.Options);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, CloseButton, IonIconsFonts.Close);
				CloseButton.Visibility = ViewStates.Gone;
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
				MAdapter = new VideoBigAdapter(Activity)
				{
					VideoList = new ObservableCollection<VideoDataObject>()
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

		private void InitToolbar(View view)
		{
			try
			{
				var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					GlobalContext.SetToolBar(toolbar, "");

					SearchView = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
					SearchView.SetOnEditorActionListener(this);
					SearchView.AfterTextChanged += SearchViewOnAfterTextChanged;

					//Change text colors
					Methods.SetColorEditText(SearchView, AppTools.IsTabDark() ? Color.White : Color.Black);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void CreateCategoryButtons()
		{
			try
			{
				int count = AppSettings.LastKeyWordList.Count;
				if (count > 0)
				{
					foreach (var text in AppSettings.LastKeyWordList)
					{
						LinearLayout ll1 = new LinearLayout(Context);

						var lp1 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, PixelUtil.Dp(Context, 48));
						lp1.SetMargins(0, 0, 0, 0);
						lp1.Gravity = GravityFlags.Center;

						ll1.LayoutParameters = lp1;
						ll1.Orientation = Orientation.Horizontal;
						//ll1.Click += CategoryOnClick;

						ImageView icon = new ImageView(Context);
						icon.SetImageResource(Resource.Drawable.pif_search_history);
						icon.SetColorFilter(Color.ParseColor("#A1A6B2"));

						var lpIcon = new LinearLayout.LayoutParams(PixelUtil.Dp(Context, 20), PixelUtil.Dp(Context, 20));
						lpIcon.SetMargins(12, 0, 0, 0);
						lpIcon.Gravity = GravityFlags.Center;

						icon.LayoutParameters = lpIcon;
						ll1.AddView(icon);

						TextView txtKeyName = new TextView(Context);
						var lpKeyName = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

						lpKeyName.SetMargins(12, 0, 0, 0);
						lpKeyName.Gravity = GravityFlags.Center;
						txtKeyName.LayoutParameters = lpKeyName;

						txtKeyName.Text = text;
						//txtKeyName.SetBackgroundResource(Resource.Drawable.round_button_gray);
						txtKeyName.SetTextColor(Color.ParseColor("#00020F"));
						TextViewCompat.SetTextAppearance(txtKeyName, Resource.Style.PifTextKeyWord);
						txtKeyName.SetPadding(0, 0, 0, 0);

						ll1.Tag = text;
						ll1.AddView(txtKeyName);
						ll1.Click += CategoryOnClick;

						CatLayout.AddView(ll1);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void CategoryOnClick(object sender, EventArgs e)
		{
			try
			{
				if (sender is LinearLayout btnCurrent)
				{
					SearchKey = SearchView.Text = btnCurrent.Tag?.ToString();

					if (string.IsNullOrEmpty(SearchKey))
						return;

					CatLayout.Visibility = ViewStates.Gone;

					SearchView.ClearFocus();

					MAdapter.VideoList.Clear();
					MAdapter.NotifyDataSetChanged();

					if (Methods.CheckConnectivity())
					{
						MRecycler.Visibility = ViewStates.Visible;
						EmptyStateLayout.Visibility = ViewStates.Gone;

						SwipeRefreshLayout.Refreshing = true;
						SwipeRefreshLayout.Enabled = true;

						MainScrollEvent.IsLoading = false;

						//Close keyboard
						HideKeyboard();

						Task.Factory.StartNew(() => StartApiService());
					}
					else
					{
						MRecycler.Visibility = ViewStates.Gone;
						EmptyStateLayout.Visibility = ViewStates.Visible;
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Events

		// Open Video  
		private void MAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item);
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
				//Get Data Api
				MAdapter.VideoList.Clear();
				MAdapter.NotifyDataSetChanged();

				MainScrollEvent.IsLoading = false;
				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		// Filter >> Get video by date (last_hour','today','this_week','this_month','this_year)
		private void FilterButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var dialogList = new MaterialAlertDialogBuilder(Activity);

				var arrayAdapter = new List<string>
				{
					Activity.GetText(Resource.String.Lbl_LastHour),
					Activity.GetText(Resource.String.Lbl_Today),
					Activity.GetText(Resource.String.Lbl_ThisWeek),
					Activity.GetText(Resource.String.Lbl_ThisMonth),
					Activity.GetText(Resource.String.Lbl_ThisYear)
				};

				dialogList.SetTitle(GetText(Resource.String.Lbl_Date));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void CloseButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				SearchKey = SearchView.Text = "";

				if (string.IsNullOrEmpty(SearchKey))
					return;

				SearchView.ClearFocus();

				MAdapter.VideoList.Clear();
				MAdapter.NotifyDataSetChanged();

				MRecycler.Visibility = ViewStates.Gone;
				EmptyStateLayout.Visibility = ViewStates.Visible;
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
				var item = MAdapter.VideoList.LastOrDefault();
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
				int countList = MAdapter.VideoList.Count;

				var (apiStatus, respond) = await RequestsAsync.Video.Search_VideosAsync(SearchKey, "10", offset, DateId);
				if (apiStatus != 200 || respond is not GetVideosListDataObject result || result.VideoList == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.VideoList.Count;
					if (respondList > 0)
					{
						result.VideoList = AppTools.ListFilter(result.VideoList);

						if (countList > 0)
						{
							foreach (var item in from item in result.VideoList let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
							{
								MAdapter.VideoList.Add(item);
							}

							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.VideoList.Count - countList); });
						}
						else
						{
							MAdapter.VideoList = new ObservableCollection<VideoDataObject>(result.VideoList);
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.VideoList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
					}
				}

				Activity?.RunOnUiThread(ShowEmptyPage);
			}
			else
			{
				//EmptyStateInflater x = new EmptyStateInflater();
				//x?.InflateLayout(this, EmptyStateInflater.Type.NoConnection);
				//if (!x.EmptyStateButton.HasOnClickListeners)
				//{
				//    x.EmptyStateButton.Click += null;
				//    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
				//}

				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		private void ShowEmptyPage()
		{
			try
			{
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;

				CatLayout.Visibility = ViewStates.Gone;
				if (MAdapter.VideoList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}
				else
				{
					MRecycler.Visibility = ViewStates.Gone;
					EmptyStateLayout.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region MaterialDialog

		public void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				if (itemString == Activity.GetString(Resource.String.Lbl_LastHour))
				{
					DateId = "last_hour";
				}
				else if (itemString == Activity.GetString(Resource.String.Lbl_Today))
				{
					DateId = "today";
				}
				else if (itemString == Activity.GetString(Resource.String.Lbl_ThisWeek))
				{
					DateId = "this_week";
				}
				else if (itemString == Activity.GetString(Resource.String.Lbl_ThisMonth))
				{
					DateId = "this_month";
				}
				else if (itemString == Activity.GetString(Resource.String.Lbl_ThisYear))
				{
					DateId = "this_year";
				}

				MAdapter.VideoList.Clear();
				MAdapter.NotifyDataSetChanged();

				SwipeRefreshLayout.Refreshing = true;
				MainScrollEvent.IsLoading = false;

				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		private void HideKeyboard()
		{
			try
			{
				var inputManager = (InputMethodManager)Context.GetSystemService(Android.Content.Context.InputMethodService);
				inputManager?.HideSoftInputFromWindow(SearchView.WindowToken, HideSoftInputFlags.None);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
		{
			if (actionId == ImeAction.Search)
			{
				try
				{
					SearchKey = v.Text;

					SearchView.ClearFocus();
					v.ClearFocus();

					MAdapter.VideoList.Clear();
					MAdapter.NotifyDataSetChanged();

					if (Methods.CheckConnectivity())
					{
						MRecycler.Visibility = ViewStates.Visible;
						EmptyStateLayout.Visibility = ViewStates.Gone;

						SwipeRefreshLayout.Refreshing = true;
						SwipeRefreshLayout.Enabled = true;

						MainScrollEvent.IsLoading = false;

						//Close keyboard
						HideKeyboard();

						Task.Factory.StartNew(() => StartApiService());
					}
					else
					{
						MRecycler.Visibility = ViewStates.Gone;
						EmptyStateLayout.Visibility = ViewStates.Visible;
					}
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}

				return true;
			}

			return false;
		}

		private void SearchViewOnAfterTextChanged(object sender, AfterTextChangedEventArgs e)
		{
			try
			{
				if (SearchView.Text.Length > 0 && !string.IsNullOrWhiteSpace(SearchView.Text))
					CloseButton.Visibility = ViewStates.Visible;
				else
					CloseButton.Visibility = ViewStates.Gone;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

	}
}