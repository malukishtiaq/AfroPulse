using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Gms.Ads;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Activities.Base
{
	public class RecyclerViewDefaultBaseFragment : Fragment
	{
		protected void ShowGoogleAds(View view, RecyclerView recyclerView)
		{
			try
			{
				var adView = view.FindViewById<AdView>(Resource.Id.adView);
				AdsGoogle.InitAdView(adView, recyclerView);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected void ShowFacebookAds(View view, RecyclerView recyclerView)
		{
			try
			{
				var containerLayout = view.FindViewById<LinearLayout>(Resource.Id.bannerContainer);

				if (AppSettings.ShowFbBannerAds)
					AdsFacebook.InitAdView(Activity, containerLayout, recyclerView);
				else if (AppSettings.ShowAppLovinBannerAds)
					AdsAppLovin.InitBannerAd(Activity, containerLayout, recyclerView);
				else
					AdsGoogle.InitBannerAdView(Activity, containerLayout, recyclerView);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}