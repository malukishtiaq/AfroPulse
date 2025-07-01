using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Google.Android.Gms.Ads;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient;
using System;
using Console = System.Console;
using Object = Java.Lang.Object;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Shorts
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class CreateShortsActivity : BaseActivity
	{
		#region Variables Basic

		private SwipeRefreshLayout SwipeRefreshLayout;
		private WebView HybridView;
		private string Url = "";
		private AdView MAdView;
		private static IValueCallback MUm;
		private static IValueCallback MUma;
		private static readonly int Fcr = 1;

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
				SetContentView(Resource.Layout.LocalWebViewLayout);

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
				DestroyBasic();
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
				var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					toolbar.Title = GetText(Resource.String.Lbl_CreateShort);
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
					switch ((int)Build.VERSION.SdkInt)
					{
						case < 23:
							LoadWebView();
							break;
						default:
							{
								if (PermissionsController.CheckPermissionStorage(this) && ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted && ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessMediaLocation) == Permission.Granted)
								{
									LoadWebView();
								}
								else
								{
									new PermissionsController(this).RequestPermission(108);
								}

								break;
							}
					}
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
				//Set WebView and Load url to be rendered on WebView
				if (!Methods.CheckConnectivity())
				{
					SwipeRefreshLayout.Refreshing = false;

					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
				else
				{
					//Set WebView   
					WebSettings webSettings = HybridView.Settings;
					HybridView.SetWebViewClient(new MyWebViewClient(this));
					HybridView.SetWebChromeClient(new MyWebChromeClient(this));

					webSettings.JavaScriptEnabled = true;
					webSettings.AllowFileAccess = true;
					webSettings.LoadsImagesAutomatically = true;
					webSettings.JavaScriptCanOpenWindowsAutomatically = true;
					webSettings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
					webSettings.DomStorageEnabled = true;
					HybridView.ClearCache(true);
					webSettings.UseWideViewPort = true;
					webSettings.LoadWithOverviewMode = true;

					webSettings.SetSupportZoom(false);
					webSettings.BuiltInZoomControls = false;
					webSettings.DisplayZoomControls = false;

					HybridView.CopyBackForwardList();
					HybridView.CanGoBackOrForward(0);

					Url = AppTools.IsTabDark()
						? InitializePlayTube.WebsiteUrl + "/upload-video?cookie=" + UserDetails.AccessToken + "&user_id=" + UserDetails.UserId + "&mode=night&type=shorts"
						: InitializePlayTube.WebsiteUrl + "/upload-video?cookie=" + UserDetails.AccessToken + "&user_id=" + UserDetails.UserId + "&mode=day&type=shorts";

					switch (AppSettings.Lang)
					{
						case "en":
							HybridView.LoadUrl(Url + "&lang=english");
							break;
						case "ar":
							HybridView.LoadUrl(Url + "&lang=arabic");
							AppSettings.FlowDirectionRightToLeft = true;
							break;
						case "de":
							HybridView.LoadUrl(Url + "&lang=german");
							break;
						case "el":
							HybridView.LoadUrl(Url + "&lang=greek");
							break;
						case "es":
							HybridView.LoadUrl(Url + "&lang=spanish");
							break;
						case "fr":
							HybridView.LoadUrl(Url + "&lang=french");
							break;
						case "it":
							HybridView.LoadUrl(Url + "&lang=italian");
							break;
						case "ja":
							HybridView.LoadUrl(Url + "&lang=japanese");
							break;
						case "nl":
							HybridView.LoadUrl(Url + "&lang=dutch");
							break;
						case "pt":
							HybridView.LoadUrl(Url + "&lang=portuguese");
							break;
						case "ro":
							HybridView.LoadUrl(Url + "&lang=romanian");
							break;
						case "ru":
							HybridView.LoadUrl(Url + "&lang=russian");
							break;
						case "sq":
							HybridView.LoadUrl(Url + "&lang=albanian");
							break;
						case "sr":
							HybridView.LoadUrl(Url + "&lang=serbian");
							break;
						case "tr":
							HybridView.LoadUrl(Url + "&lang=turkish");
							break;
						default:
							HybridView.LoadUrl(Url);
							break;
					}

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
				switch (addEvent)
				{
					// true +=  // false -=
					case true:
						SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
						break;
					default:
						SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
		private void DestroyBasic()
		{
			try
			{
				AdsGoogle.LifecycleAdView(MAdView, "Destroy");

				HybridView = null;
				SwipeRefreshLayout = null;
				MAdView = null;
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

		#region Result & Permissions

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			try
			{
				base.OnActivityResult(requestCode, resultCode, data);

				if ((int)Build.VERSION.SdkInt is >= 21)
				{
					Android.Net.Uri[] results = null;
					if (resultCode == Result.Ok)
						if (requestCode == Fcr)
						{
							if (MUma == null) return;

							var dataString = data?.Data?.ToString();
							if (dataString != null)
							{
								results = new[]
								{
									Android.Net.Uri.Parse(dataString)
								};
							}
						}

					MUma.OnReceiveValue(results);
					MUma = null;
				}
				else
				{
					if (requestCode == Fcr)
					{
						if (MUm == null) return;

						Android.Net.Uri result = data == null || resultCode != Result.Ok ? null : data.Data;
						MUm.OnReceiveValue(result);
						MUm = null;
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			try
			{
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

				switch (requestCode)
				{
					case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
						LoadWebView();
						break;
					case 108:
						Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
						Finish();
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		private class MyWebViewClient : WebViewClient, IValueCallback
		{
			private readonly CreateShortsActivity MActivity;
			public MyWebViewClient(CreateShortsActivity mActivity)
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
					else if (request.Url.ToString().Contains("/shorts/"))
					{
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
					if (MActivity.SwipeRefreshLayout != null)
					{
						MActivity.SwipeRefreshLayout.Refreshing = false;
						MActivity.SwipeRefreshLayout.Enabled = false;
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
					if (MActivity.SwipeRefreshLayout != null)
					{
						MActivity.SwipeRefreshLayout.Refreshing = false;
						MActivity.SwipeRefreshLayout.Enabled = false;
					}

					Methods.DisplayReportResultTrack(e);
				}
			}

			public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
			{
				try
				{
					base.OnReceivedError(view, request, error);
					if (MActivity.SwipeRefreshLayout != null)
					{
						MActivity.SwipeRefreshLayout.Refreshing = false;
						MActivity.SwipeRefreshLayout.Enabled = false;
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
					if (MActivity.SwipeRefreshLayout != null)
					{
						MActivity.SwipeRefreshLayout.Refreshing = false;
						MActivity.SwipeRefreshLayout.Enabled = false;
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

		private class MyWebChromeClient : WebChromeClient
		{
			private readonly Activity MActivity;

			public MyWebChromeClient(Activity mActivity)
			{
				MActivity = mActivity;
			}

			[Android.Runtime.Register("onShowFileChooser", "(Landroid/webkit/WebView;Landroid/webkit/ValueCallback;Landroid/webkit/WebChromeClient$FileChooserParams;)Z", "GetOnShowFileChooser_Landroid_webkit_WebView_Landroid_webkit_ValueCallback_Landroid_webkit_WebChromeClient_FileChooserParams_Handler", ApiSince = 21)]
			public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
			{
				try
				{
					MUma?.OnReceiveValue(null);

					MUma = filePathCallback;

					IValueCallback filePath;

					if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
					{
						filePath = filePathCallback;
						Intent takePictureIntent = null;
						Intent takeVideoIntent = null;

						bool includeVideo = false;
						bool includePhoto = false;
						string fileType = "*/*";
						//checking the accept parameter to determine which intent(s) to include 
						foreach (var acceptTypes in fileChooserParams.GetAcceptTypes())
						{
							var splitTypes = acceptTypes.Split(", ?+");
							/*-- although it's an array, it still seems to be the whole value; split it out into chunks so that we can detect multiple values --*/
							foreach (var acceptType in splitTypes)
							{
								switch (acceptType)
								{
									case "*/*":
										includePhoto = true;
										includeVideo = true;
										fileType = "*/*";
										break;
									case "image/*":
										includePhoto = true;
										fileType = "image/*";
										break;
									case "video/*":
										includeVideo = true;
										fileType = "video/*";
										break;
								}
							}
						}

						if (fileChooserParams.GetAcceptTypes()?.Length == 0)
						{
							//no `accept` parameter was specified, allow both photo and video
							includePhoto = true;
							includeVideo = true;
						}

						if (includePhoto)
						{
							takePictureIntent = (int)Build.VERSION.SdkInt switch
							{
								<= 25 => new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri),
								_ => Android.OS.Environment.GetExternalStorageState(null)!.Equals(Android.OS.Environment.MediaMounted)
									? new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri)
									: new Intent(Intent.ActionPick, MediaStore.Images.Media.InternalContentUri)
							};

							takePictureIntent?.SetType("image/*");
							takePictureIntent?.PutExtra("return-data", true); //added snippet

							if (Build.VERSION.SdkInt > BuildVersionCodes.Q)
								takePictureIntent?.SetAction(Intent.ActionGetContent);
						}

						if (includeVideo)
						{
							takeVideoIntent = (int)Build.VERSION.SdkInt switch
							{
								<= 25 => new Intent(Intent.ActionPick, MediaStore.Video.Media.ExternalContentUri),
								_ => Android.OS.Environment.GetExternalStorageState(null)!.Equals(Android.OS.Environment.MediaMounted)
									? new Intent(Intent.ActionPick, MediaStore.Video.Media.ExternalContentUri)
									: new Intent(Intent.ActionPick, MediaStore.Video.Media.InternalContentUri)
							};
							takeVideoIntent?.SetType("video/*");
							takeVideoIntent?.PutExtra("return-data", true); //added snippet

							if (Build.VERSION.SdkInt > BuildVersionCodes.Q)
								takeVideoIntent?.SetAction(Intent.ActionGetContent);

							takeVideoIntent?.AddFlags(ActivityFlags.GrantReadUriPermission);

						}

						Intent contentSelectionIntent = new Intent(Intent.ActionGetContent);
						contentSelectionIntent?.AddCategory(Intent.CategoryOpenable);
						contentSelectionIntent?.SetType(fileType);

						Intent[] intentArray;
						if (takePictureIntent != null && takeVideoIntent != null)
						{
							intentArray = new Intent[] { takePictureIntent, takeVideoIntent };
						}
						else if (takePictureIntent != null)
						{
							intentArray = new Intent[] { takePictureIntent };
						}
						else if (takeVideoIntent != null)
						{
							intentArray = new Intent[] { takeVideoIntent };
						}
						else
						{
							intentArray = Array.Empty<Intent>();
						}

						Intent chooserIntent = new Intent(Intent.ActionChooser);
						chooserIntent?.PutExtra(Intent.ExtraIntent, contentSelectionIntent);
						chooserIntent?.PutExtra(Intent.ExtraTitle, "File chooser");
						chooserIntent?.PutExtra(Intent.ExtraInitialIntents, intentArray);
						MActivity.StartActivityForResult(chooserIntent, Fcr);

						return true;

					}
					return false;
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
					return false;
				}
			}
		}

	}
}