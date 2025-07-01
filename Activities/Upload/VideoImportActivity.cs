using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Google.Android.Gms.Ads;
using PlayTube.Activities.Base;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Upload
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class VideoImportActivity : BaseActivity
	{
		#region Variables Basic

		private static SwipeRefreshLayout SwipeRefreshLayout;
		private static WebView HybridView;
		private string Url = "";
		private AdView MAdView;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				Methods.App.FullScreenApp(this);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.LocalWebViewLayout);

				if (AppTools.IsTabDark())
				{
					Url = InitializePlayTube.WebsiteUrl + "/import-video?cookie=" + UserDetails.Cookie + "&mode=night";
				}
				else
				{
					Url = InitializePlayTube.WebsiteUrl + "/import-video?cookie=" + UserDetails.Cookie;
				}

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				SetWebView();
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
				HybridView = FindViewById<WebView>(Resource.Id.LocalWebView);
				SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

				MAdView = FindViewById<AdView>(Resource.Id.adView);
				AdsGoogle.InitAdView(MAdView, null);
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
				Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					toolbar.Title = GetText(Resource.String.Lbl_Import);
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

		private void LoadWebView()
		{
			try
			{
				//Set WebView 
				WebSettings webSettings = HybridView.Settings;
				HybridView.SetWebViewClient(new MyWebViewClient(this));
				HybridView.SetWebChromeClient(new WebChromeClient());

				webSettings.JavaScriptEnabled = true;
				webSettings.AllowFileAccess = true;
				webSettings.LoadsImagesAutomatically = true;
				webSettings.JavaScriptCanOpenWindowsAutomatically = true;
				webSettings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
				webSettings.DomStorageEnabled = true;
				webSettings.UseWideViewPort = true;
				webSettings.LoadWithOverviewMode = true;
				webSettings.SetSupportZoom(false);
				webSettings.BuiltInZoomControls = false;
				webSettings.DisplayZoomControls = false;

				HybridView.ClearCache(true); HybridView.Settings.LoadWithOverviewMode = true;

				HybridView.CopyBackForwardList();
				HybridView.CanGoBackOrForward(0);

				//Load url to be rendered on WebView
				HybridView.LoadUrl(Url);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetWebView()
		{
			try
			{
				//Set WebView and Load url to be rendered on WebView
				if (!Methods.CheckConnectivity())
				{
					SwipeRefreshLayout.Refreshing = false;
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
				else
				{
					//Set WebView
					LoadWebView();
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

		#endregion

		#region Events

		//Event Refresh Data Page
		private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
		{
			try
			{
				HybridView.Reload();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		private class MyWebViewClient : WebViewClient, IValueCallback
		{
			private readonly VideoImportActivity MActivity;
			public MyWebViewClient(VideoImportActivity mActivity)
			{
				MActivity = mActivity;
			}

			public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
			{
				try
				{
					if (request.Url.ToString() == MActivity.Url)
					{
						view.LoadUrl(request.Url.ToString());
					}
					else if (request.Url.ToString().Contains("watch"))
					{
						var con = TabbedMainActivity.GetInstance()?.MyChannelFragment?.VideosFragment;
						if (con != null)
						{
							con.SwipeRefreshLayout.Refreshing = true;

							con.MAdapter.VideoList.Clear();
							con.MAdapter.NotifyDataSetChanged();

							con.MainScrollEvent.IsLoading = false;

							//https://demo.playtubescript.com/watch/JaELhCpu899UHew
							var videoId = request.Url.ToString()!.Split("/").Last();

							Task.Factory.StartNew(() =>
							{
								con.CheckVideoStatus(videoId);
								con.StartApiService();
							});
						}
						MActivity.Finish();
					}
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
				return false;
			}

			public override void OnPageStarted(WebView view, string url, Bitmap favicon)
			{
				try
				{
					base.OnPageStarted(view, url, favicon);

					view.Settings.JavaScriptEnabled = true;
					view.Settings.DomStorageEnabled = true;
					view.Settings.AllowFileAccess = true;
					view.Settings.JavaScriptCanOpenWindowsAutomatically = true;
					view.Settings.UseWideViewPort = true;
					view.Settings.LoadWithOverviewMode = true;
					view.Settings.SetSupportZoom(false);
					view.Settings.BuiltInZoomControls = false;
					view.Settings.DisplayZoomControls = false;
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}

			public override void OnPageFinished(WebView view, string url)
			{
				try
				{
					base.OnPageFinished(view, url);
					if (SwipeRefreshLayout != null)
					{
						SwipeRefreshLayout.Refreshing = false;
						SwipeRefreshLayout.Enabled = false;
					}

					const string js = "javascript:" +
									  "var elements = document.getElementsByClassName('navbar navbar-findcond navbar-fixed-top header-layout');" +
									  " elements[0].style.display = 'none'; " +
									  "$('.header').hide();" +
									  "$('.pt_footer').hide();" +
									  "$('.btn-fab-floating').hide();" +
									  "$('.content-container').css('margin-top', '0');" +
									  "$('.wo_about_wrapper_parent').css('top', '0');";

					if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
					{
						view.EvaluateJavascript(js, this);
					}
					else
					{
						view.LoadUrl(js);
					}
				}
				catch (Exception e)
				{
					if (SwipeRefreshLayout != null)
					{
						SwipeRefreshLayout.Refreshing = false;
						SwipeRefreshLayout.Enabled = false;
					}
					Methods.DisplayReportResultTrack(e);
				}
			}

			public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
			{
				try
				{
					base.OnReceivedError(view, request, error);
					if (SwipeRefreshLayout != null)
					{
						SwipeRefreshLayout.Refreshing = false;
						SwipeRefreshLayout.Enabled = false;
					}
					const string js = "javascript:" +
									  "var elements = document.getElementsByClassName('navbar navbar-findcond navbar-fixed-top header-layout');" +
									  " elements[0].style.display = 'none'; " +
									  "$('.header').hide();" +
									  "$('.pt_footer').hide();" +
									  "$('.btn-fab-floating').hide();" +
									  "$('.content-container').css('margin-top', '0');" +
									  "$('.wo_about_wrapper_parent').css('top', '0');";

					if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
					{
						view.EvaluateJavascript(js, this);
					}
					else
					{
						view.LoadUrl(js);
					}
				}
				catch (Exception e)
				{
					if (SwipeRefreshLayout != null)
					{
						SwipeRefreshLayout.Refreshing = false;
						SwipeRefreshLayout.Enabled = false;
					}
					Methods.DisplayReportResultTrack(e);
				}
			}

			public void OnReceiveValue(Object value)
			{
				try
				{

				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}
		}
	}
}