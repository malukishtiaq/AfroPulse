using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.Fragment.App;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PlayTube.Activities.PlayersView
{
	public class ThirdPartyPlayersFragment : Fragment
	{
		public WebView WebViewPlayer;
		private ProgressBar ProgressBarLoader;
		private RelativeLayout ErrorPage;
		private TextView ErrorTextView;
		private static View MainView;
		public static bool IsPlay;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				MainView = inflater?.Inflate(Resource.Layout.ThirdPartyPlayersLayout, container, false);
				return MainView;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
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
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnResume()
		{
			try
			{
				base.OnResume();
				WebViewPlayer?.OnResume();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnPause()
		{
			try
			{
				base.OnPause();
				WebViewPlayer?.OnPause();
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
				MainView = null;
				IsPlay = false;
				base.OnDestroy();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public async void SetVideoIframe(VideoDataObject videoObject)
		{
			try
			{
				if (WebViewPlayer == null)
					InitComponent(MainView);

				IsPlay = false;

				await Task.Delay(1000);
				//DisplayMetrics displayMetrics = new DisplayMetrics();
				//Activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
				//int width = Resources?.Configuration?.ScreenWidthDp;
				//int height = Resources?.Configuration?.ScreenHeightDp;
				//Methods.DisplayReportResultTrack(width+ " , " + height);

				string videoIframe = string.Empty;

				if (!string.IsNullOrEmpty(videoObject.Vimeo))
				{
					videoObject.VideoType = "Vimeo";
					videoIframe = "https://player.vimeo.com/video/" + videoObject.Vimeo + "?autoplay=1";
				}
				else if (!string.IsNullOrEmpty(videoObject.Twitch))
				{
					videoObject.VideoType = "Twitch";
					videoIframe = "https://player.twitch.tv/?autoplay=true&video=" + videoObject.Twitch;
				}
				else if (!string.IsNullOrEmpty(videoObject.Daily))
				{
					videoObject.VideoType = "Daily";
					videoIframe = "https://www.dailymotion.com/embed/video/" + videoObject.Daily + "?autoPlay=1";
				}
				else if (!string.IsNullOrEmpty(videoObject.Ok))
				{
					videoObject.VideoType = "Ok";
					videoIframe = "https://ok.ru/videoembed/" + videoObject.Ok + "?autoplay=1";
				}
				else if (!string.IsNullOrEmpty(videoObject.Facebook))
				{
					videoObject.VideoType = "Facebook";

					var content = "<iframe src='https://www.facebook.com/plugins/video.php?height=600&href=https://www.facebook.com/" + videoObject.Facebook + "/&show_text=false'";
					content += "width='100%' height='600' style='border:none;overflow:hidden'";
					content += "scrolling='no' frameborder='0' allowfullscreen='true'";
					content += "allow='autoplay; clipboard-write; encrypted-media; picture-in-picture; web-share'";
					content += "allowFullScreen='true'>";
					content += "</iframe>";

					videoIframe = "<!DOCTYPE html>";
					videoIframe += "<head><title></title>" + "</head>";
					videoIframe += "<body>" + content + "</body>";
					videoIframe += "</html>";

					//videoIframe = "https://www.facebook.com/video/embed?video_id=" + videoObject.Facebook;
					WebViewPlayer.LoadDataWithBaseURL(null, videoIframe, "text/html", "UTF-8", null);
					return;
				}

				if (!string.IsNullOrEmpty(videoIframe))
					WebViewPlayer.LoadUrl(videoIframe);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitComponent(View view)
		{
			try
			{
				WebViewPlayer = (WebView)view.FindViewById(Resource.Id.myWebView);
				WebSettings webSettings = WebViewPlayer.Settings;
				webSettings.JavaScriptEnabled = true;
				WebViewPlayer.HorizontalScrollBarEnabled = false;
				WebViewPlayer.VerticalScrollBarEnabled = false;
				WebViewPlayer.SetWebViewClient(new VideoWebViewClient(this));
				WebViewPlayer.SetWebChromeClient(new WebChromeClient());
				WebViewPlayer.SetInitialScale(1);
				webSettings.AllowFileAccess = true;
				webSettings.SetPluginState(WebSettings.PluginState.On);
				webSettings.SetPluginState(WebSettings.PluginState.OnDemand);
				webSettings.LoadWithOverviewMode = true;
				webSettings.UseWideViewPort = true;
				webSettings.DomStorageEnabled = true;
				webSettings.LoadsImagesAutomatically = true;
				webSettings.JavaScriptCanOpenWindowsAutomatically = true;
				webSettings.MediaPlaybackRequiresUserGesture = false;
				webSettings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
				WebViewPlayer.ScrollBarStyle = ScrollbarStyles.InsideOverlay;

				ProgressBarLoader = view.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
				ProgressBarLoader.Visibility = ViewStates.Visible;

				ErrorPage = view.FindViewById<RelativeLayout>(Resource.Id.ErrorPage);
				ErrorPage.Visibility = ViewStates.Gone;

				ErrorTextView = view.FindViewById<TextView>(Resource.Id.errorTextView);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private class VideoWebViewClient : WebViewClient
		{
			private readonly ThirdPartyPlayersFragment MActivity;
			public VideoWebViewClient(ThirdPartyPlayersFragment mActivity)
			{
				MActivity = mActivity;
			}

			public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
			{
				view.LoadUrl(request.Url.ToString());
				return true;
			}

			public override void OnPageFinished(WebView view, string url)
			{
				try
				{
					base.OnPageFinished(view, url);

					if (MActivity.ProgressBarLoader.Visibility == ViewStates.Visible)
						MActivity.ProgressBarLoader.Visibility = ViewStates.Gone;

					IsPlay = true;
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}


			public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
			{
				try
				{
					base.OnReceivedError(view, request, error);

					MActivity.WebViewPlayer.Visibility = ViewStates.Gone;
					MActivity.ErrorPage.Visibility = ViewStates.Visible;

					if (MActivity.ProgressBarLoader.Visibility == ViewStates.Visible)
						MActivity.ProgressBarLoader.Visibility = ViewStates.Gone;

					string textError = MActivity.GetString(Resource.String.Lbl_Error) + ": ";
					switch (error.ErrorCode)
					{
						case ClientError.BadUrl:
							textError += "Bad Url";
							break;
						case ClientError.Connect:
							textError += "Connect";
							break;
						case ClientError.FailedSslHandshake:
							textError += "Failed Ssl Handshake";
							break;
						case ClientError.File:
							textError += "File";
							break;
						case ClientError.FileNotFound:
							textError += "File Not Found";
							break;
						case ClientError.HostLookup:
							textError += "Host Lookup";
							break;
						case ClientError.ProxyAuthentication:
							textError += "Proxy Authentication";
							break;
						case ClientError.Timeout:
							textError += "Timeout";
							break;
						case ClientError.TooManyRequests:
							textError += "Too Many Requests";
							break;
						case ClientError.Unknown:
							textError += "Unknown";
							break;
						case ClientError.UnsafeResource:
							textError += "Unsafe Resource";
							break;
						case ClientError.UnsupportedScheme:
							textError += "Unsupported Scheme";
							break;
						case ClientError.Io:
							textError += "Io";
							break;
						default:
							textError += error.Description;
							break;
					}
					MActivity.ErrorTextView.Text = textError;

				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}
		}
	}
}