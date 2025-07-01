using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using PlayTube.Activities.Article.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Article
{
	public class ArticlesFragment : Fragment, TextView.IOnEditorActionListener
	{
		#region Variables Basic

		private AppCompatButton AddButton;

		private AutoCompleteTextView SearchView;

		private TabLayout Tabs;

		private ArticlesAdapter MAdapterCat, MAdapterPopular;
		public ArticleRowAdapter MAdapterBlog;
		private RecyclerView MRecyclerCat, MRecyclerBlog;
		private LinearLayoutManager LayoutManager;
		private RecyclerViewOnScrollListener MainScrollEvent;

		private ViewStub PopularViewStub;
		private View PopularInflated;

		private TabbedMainActivity MainContext;
		private string CategoryId, Keyword;
		private static ArticlesFragment Instance;

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
				View view = inflater?.Inflate(Resource.Layout.ArticlesLayout, container, false);
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
				SetUpTabs();
				SetRecyclerViewAdapters();

				MAdapterCat.ItemClick += MAdapterCatOnItemClick;
				MAdapterPopular.ItemClick += MAdapterPopularOnItemClick;
				MAdapterBlog.ItemClick += MAdapterBlogOnItemClick;
				AddButton.Click += AddButtonOnClick;

				//Get Data Api
				Task.Factory.StartNew(() => { StartApiService(); });

				AdsGoogle.Ad_Interstitial(Activity);
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
				AddButton = (AppCompatButton)view.FindViewById(Resource.Id.create);

				SearchView = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
				SearchView.SetOnEditorActionListener(this);

				//Change text colors
				SearchView.SetHintTextColor(Color.ParseColor(AppSettings.MainColor));
				SearchView.SetTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

				Tabs = view.FindViewById<TabLayout>(Resource.Id.tab_blog);
				Tabs.TabSelected += TabsOnTabSelected;

				MRecyclerCat = (RecyclerView)view.FindViewById(Resource.Id.RecyclerBlogCat);

				PopularViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubPopular);

				MRecyclerBlog = (RecyclerView)view.FindViewById(Resource.Id.RecyclerBlog);

				AdManagerAdView AdManagerAdView = view.FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
				AdsGoogle.InitAdManagerAdView(AdManagerAdView);
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
				MAdapterCat = new ArticlesAdapter(Activity, this)
				{
					ArticlesList = new ObservableCollection<ArticleDataObject>()
				};
				LayoutManager = new LinearLayoutManager(Context, LinearLayoutManager.Horizontal, false);
				MRecyclerCat.SetLayoutManager(LayoutManager);
				MRecyclerCat.HasFixedSize = true;
				MRecyclerCat.SetItemViewCacheSize(10);
				MRecyclerCat.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<ArticleDataObject>(Activity, MAdapterCat, sizeProvider, 10);
				MRecyclerCat.AddOnScrollListener(preLoader);
				MRecyclerCat.SetAdapter(MAdapterCat);

				RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
				MainScrollEvent = xamarinRecyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
				MRecyclerCat.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;

				//========================
				MAdapterPopular = new ArticlesAdapter(Activity, this)
				{
					ArticlesList = new ObservableCollection<ArticleDataObject>()
				};

				//========================

				MAdapterBlog = new ArticleRowAdapter(Activity)
				{
					ArticlesList = new ObservableCollection<ArticleDataObject>()
				};
				MRecyclerBlog.SetLayoutManager(new LinearLayoutManager(Context));
				MRecyclerBlog.HasFixedSize = true;
				MRecyclerBlog.SetItemViewCacheSize(10);
				MRecyclerBlog.GetLayoutManager().ItemPrefetchEnabled = true;
				MRecyclerBlog.AddOnScrollListener(preLoader);
				MRecyclerBlog.SetAdapter(MAdapterBlog);
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
					string title = Context.GetString(Resource.String.Lbl_ExploreArticle);
					MainContext.SetToolBar(toolbar, title);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetUpTabs()
		{
			try
			{
				if (CategoriesController.ListCategories.Count > 0)
				{
					CategoryId = CategoriesController.ListCategories.FirstOrDefault()?.Id;

					foreach (var cat in CategoriesController.ListCategories)
					{
						Tabs.AddTab(Tabs.NewTab().SetText(cat.Name));
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static ArticlesFragment GetInstance()
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

		#region Events

		private void TabsOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
		{
			try
			{
				//e.Tab.Position
				if (!string.IsNullOrEmpty(e.Tab?.Text))
				{
					// Get Articles By Categories
					CategoryId = CategoriesController.ListCategories.FirstOrDefault(categories => categories.Name == e.Tab?.Text)?.Id;

					if (MAdapterCat != null)
					{
						MAdapterCat.ArticlesList.Clear();
						MAdapterCat.NotifyDataSetChanged();

						MainScrollEvent.IsLoading = false;

						if (!Methods.CheckConnectivity())
							Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						else
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataByCategoriesAsync("0") });
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void AddButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(Context, typeof(CreateBlogActivity));
				Context.StartActivity(intent);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MAdapterPopularOnItemClick(object sender, ArticlesAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position >= 0)
				{
					var item = MAdapterPopular.GetItem(e.Position);
					if (item != null)
					{
						var intent = new Intent(Context, typeof(ShowArticleActivity));
						intent.PutExtra("ItemArticle", JsonConvert.SerializeObject(item));
						Context.StartActivity(intent);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MAdapterBlogOnItemClick(object sender, ArticleRowAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position >= 0)
				{
					var item = MAdapterBlog.GetItem(e.Position);
					if (item != null)
					{
						var intent = new Intent(Context, typeof(ShowArticleActivity));
						intent.PutExtra("ItemArticle", JsonConvert.SerializeObject(item));
						Context.StartActivity(intent);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MAdapterCatOnItemClick(object sender, ArticlesAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position >= 0)
				{
					var item = MAdapterCat.GetItem(e.Position);
					if (item != null)
					{
						var intent = new Intent(Context, typeof(ShowArticleActivity));
						intent.PutExtra("ItemArticle", JsonConvert.SerializeObject(item));
						Context.StartActivity(intent);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OpenChannel(ArticleDataObject item)
		{
			try
			{
				MainContext.ShowUserChannelFragment(item.UserData, item.UserId.ToString());
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Scroll

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
		{
			try
			{
				//Code get last id where LoadMore >>
				var item = MAdapterCat.ArticlesList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataByCategoriesAsync(item.Id.ToString()) });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		public void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadMostPopularAsync, () => LoadDataAsync(), () => LoadDataByCategoriesAsync(offset) });
		}

		private async Task LoadMostPopularAsync()
		{
			if (Methods.CheckConnectivity())
			{
				int countList = MAdapterPopular.ArticlesList.Count;

				var (apiStatus, respond) = await RequestsAsync.Articles.GetMostPopularArticlesAsync();
				if (apiStatus != 200 || respond is not GetUsersArticlesObject result || result.ArticleList == null)
				{
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.ArticleList.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							foreach (var item in from item in result.ArticleList let check = MAdapterPopular.ArticlesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
							{
								MAdapterPopular.ArticlesList.Add(item);
							}

							Activity?.RunOnUiThread(() =>
							{
								MAdapterPopular.NotifyItemRangeInserted(countList, MAdapterPopular.ArticlesList.Count - countList);
							});
						}
						else
						{
							MAdapterPopular.ArticlesList = new ObservableCollection<ArticleDataObject>(result.ArticleList);

							Activity?.RunOnUiThread(() =>
							{
								PopularInflated ??= PopularViewStub.Inflate();

								TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
								recyclerInflater.InflateLayout<UserDataObject>(Activity, PopularInflated, MAdapterPopular, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_MostPopular), Resource.Drawable.icon_file_vector);
							});
						}
					}
				}
			}
			else
			{
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (Methods.CheckConnectivity())
			{
				int countList = MAdapterBlog.ArticlesList.Count;

				var (apiStatus, respond) = await RequestsAsync.Articles.GetArticlesAsync("10", offset, "", Keyword);
				if (apiStatus != 200 || respond is not GetUsersArticlesObject result || result.ArticleList == null)
				{
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.ArticleList.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							foreach (var item in from item in result.ArticleList let check = MAdapterBlog.ArticlesList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
							{
								MAdapterBlog.ArticlesList.Add(item);
							}

							Activity?.RunOnUiThread(() => { MAdapterBlog.NotifyItemRangeInserted(countList, MAdapterBlog.ArticlesList.Count - countList); });
						}
						else
						{
							MAdapterBlog.ArticlesList = new ObservableCollection<ArticleDataObject>(result.ArticleList);
							Activity?.RunOnUiThread(() => { MAdapterBlog.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapterBlog.ArticlesList.Count > 10 && !MRecyclerBlog.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArticle), ToastLength.Short)?.Show();
					}
				}
			}
			else
			{
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
		}

		private async Task LoadDataByCategoriesAsync(string offset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;
				int countList = MAdapterCat.ArticlesList.Count;

				var (apiStatus, respond) = await RequestsAsync.Articles.GetArticlesAsync("10", offset, CategoryId);
				if (apiStatus != 200 || respond is not GetUsersArticlesObject result || result.ArticleList == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.ArticleList.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							foreach (var item in from item in result.ArticleList let check = MAdapterCat.ArticlesList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
							{
								MAdapterCat.ArticlesList.Add(item);
							}

							Activity?.RunOnUiThread(() => { MAdapterCat.NotifyItemRangeInserted(countList, MAdapterCat.ArticlesList.Count - countList); });
						}
						else
						{
							MAdapterCat.ArticlesList = new ObservableCollection<ArticleDataObject>(result.ArticleList);
							Activity?.RunOnUiThread(() => { MAdapterCat.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapterCat.ArticlesList.Count > 10 && !MRecyclerCat.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArticle), ToastLength.Short)?.Show();
					}
				}

				MainScrollEvent.IsLoading = false;
			}
			else
			{
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		#endregion

		public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
		{
			if (actionId == ImeAction.Search)
			{
				try
				{
					Keyword = v.Text;

					SearchView.ClearFocus();
					v.ClearFocus();

					MAdapterBlog.ArticlesList.Clear();
					MAdapterBlog.NotifyDataSetChanged();

					MRecyclerBlog.Visibility = ViewStates.Visible;

					//Close keyboard
					HideKeyboard();

					if (!Methods.CheckConnectivity())
						Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					else
						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync() });
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}

				return true;
			}

			return false;
		}

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

	}
}