using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Com.Google.Android.Gms.Ads;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.SuperTextLibrary;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PlayTube.Activities.Channel.Tab
{
	public class ChAboutFragment : Fragment
	{
		#region  Variables Basic

		private TabbedMainActivity GlobalContext;

		private SuperTextView TxtAbout;
		private TextView TxtGender, TxtEmail, TxtFacebook, TxtInstagram, TxtTwitter, TxtBlock, IconMonetization, TxtMonetization;
		private RelativeLayout EmailLayout, BlockLiner, FacebookLayout, InstagramLayout, TwitterLayout;
		private LinearLayout LinearMonetization1, LinearMonetization2;

		private AdView MAdView;
		private string IdChannel;
		private UserDataObject DataChannel;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Create your fragment here
			GlobalContext = (TabbedMainActivity)Activity;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				View view = inflater?.Inflate(Resource.Layout.ChAboutLayout, container, false);
				return view;
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
				IdChannel = Arguments?.GetString("ChannelId");

				InitComponent(view);

				Task.Factory.StartNew(StartApiService);
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
				TxtAbout = (SuperTextView)view.FindViewById(Resource.Id.AboutText);
				TxtGender = (TextView)view.FindViewById(Resource.Id.genderText);
				TxtEmail = (TextView)view.FindViewById(Resource.Id.emailText);
				TxtFacebook = (TextView)view.FindViewById(Resource.Id.facebookText);
				TxtInstagram = (TextView)view.FindViewById(Resource.Id.instagramText);
				TxtTwitter = (TextView)view.FindViewById(Resource.Id.twitterText);
				TxtBlock = (TextView)view.FindViewById(Resource.Id.BlockText);

				EmailLayout = (RelativeLayout)view.FindViewById(Resource.Id.emailLayout);
				FacebookLayout = (RelativeLayout)view.FindViewById(Resource.Id.facebookLayout);
				InstagramLayout = (RelativeLayout)view.FindViewById(Resource.Id.instagramLayout);
				TwitterLayout = (RelativeLayout)view.FindViewById(Resource.Id.twitterLayout);
				BlockLiner = (RelativeLayout)view.FindViewById(Resource.Id.BlockLiner);

				LinearMonetization1 = (LinearLayout)view.FindViewById(Resource.Id.MonetizationLayout1);
				LinearMonetization2 = (LinearLayout)view.FindViewById(Resource.Id.MonetizationLayout2);

				IconMonetization = (TextView)view.FindViewById(Resource.Id.MonetizationIcon);
				TxtMonetization = (TextView)view.FindViewById(Resource.Id.MonetizationText);

				MAdView = view.FindViewById<AdView>(Resource.Id.adView);
				AdsGoogle.InitAdView(MAdView, null);

				if (IdChannel == UserDetails.UserId)
				{
					BlockLiner.Visibility = ViewStates.Gone;

					LinearMonetization1.Visibility = ViewStates.Visible;
					LinearMonetization2.Visibility = ViewStates.Visible;

					FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMonetization, IonIconsFonts.LogoUsd);
				}
				else
				{
					LinearMonetization1.Visibility = ViewStates.Gone;
					LinearMonetization2.Visibility = ViewStates.Gone;

					BlockLiner.Visibility = ViewStates.Visible;
				}

				if (!AppSettings.ShowEmailAccount)
				{
					EmailLayout.Visibility = ViewStates.Gone;
				}

				FacebookLayout.Click += FacebookLayoutOnClick;
				InstagramLayout.Click += InstagramLayoutOnClick;
				TwitterLayout.Click += TwitterLayoutOnClick;
				BlockLiner.Click += BlockLinerOnClick;

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private async void BlockLinerOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Long)?.Show();

					var (apiStatus, respond) = await RequestsAsync.Global.BlockUnBlockUserAsync(IdChannel).ConfigureAwait(false);
					if (apiStatus != 200)
						Methods.DisplayReportResult(Activity, respond);

					GlobalContext.FragmentNavigatorBack();
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

		private void TwitterLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
					new IntentController(GlobalContext).OpenTwitterIntent(DataChannel.Twitter);
				else
					Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void InstagramLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
					new IntentController(GlobalContext).OpenInstagramIntent(DataChannel.Instagram);
				else
					Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void FacebookLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
					new IntentController(GlobalContext).OpenFacebookIntent(DataChannel.Facebook);
				else
					Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		private void StartApiService()
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadDataAsync });
		}

		private async Task LoadDataAsync()
		{
			if (Methods.CheckConnectivity())
			{
				DataChannel = await ApiRequest.GetChannelData(Activity, IdChannel);
				if (DataChannel != null)
				{
					Activity?.RunOnUiThread(() =>
					{
						try
						{
							TextSanitizer text = new TextSanitizer(TxtAbout, Activity, GlobalContext);
							text.Load(AppTools.GetAboutFinal(DataChannel));

							if (DataChannel.Gender == "male" || DataChannel.Gender == "Male")
							{
								TxtGender.Text = GetText(Resource.String.Radio_Male);
							}
							else
							{
								TxtGender.Text = GetText(Resource.String.Radio_Female);
							}

							TxtEmail.Text = DataChannel.Email;

							FacebookLayout.Visibility = string.IsNullOrEmpty(DataChannel.Facebook) ? ViewStates.Gone : ViewStates.Visible;
							InstagramLayout.Visibility = string.IsNullOrEmpty(DataChannel.Instagram) ? ViewStates.Gone : ViewStates.Visible;
							TwitterLayout.Visibility = string.IsNullOrEmpty(DataChannel.Twitter) ? ViewStates.Gone : ViewStates.Visible;

							if (IdChannel == UserDetails.UserId)
							{
								var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
								var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
								Console.WriteLine(currencyIcon);
								TxtMonetization.Text = DataChannel.Balance + " " + currency;
							}
							else
							{
								TxtBlock.Text = GetText(Resource.String.Lbl_Block) + " " + AppTools.GetNameFinal(DataChannel);
							}
						}
						catch (Exception e)
						{
							Methods.DisplayReportResultTrack(e);
						}
					});
				}
			}
			else
			{
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
		}

		#endregion

	}
}