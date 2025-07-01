using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.Dialog;
using PlayTube.Activities.Models;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using Exception = System.Exception;

namespace PlayTube.Activities.PlayersView
{
	public class RestrictedVideoFragment : Fragment
	{
		#region Variables Basic

		public TextView RestrictedTextView;
		public ImageView ImageVideo, RestrictedIcon;
		public AppCompatButton PurchaseButton;
		private VideoDataObject VideoObject;
		private string RestrictionText, ImageUrl;
		private TabbedMainActivity GlobalContext;
		private VideoDataWithEventsLoader VideoControl;

		#endregion

		#region General
		 
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				View view = inflater?.Inflate(Resource.Layout.RestrictedVideoLayout, container, false);
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

                GlobalContext = TabbedMainActivity.GetInstance();
				VideoControl = GlobalContext.VideoDataWithEventsLoader;

				//Get Value And Set Toolbar
				InitComponent(view);

				//if (!string.IsNullOrWhiteSpace(RestrictionText) && VideoObject != null)
				//	LoadRestriction(RestrictionText, ImageUrl, VideoObject);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{ 
				ImageVideo = (ImageView)view.FindViewById(Resource.Id.Imagevideo);
				RestrictedIcon = (ImageView)view.FindViewById(Resource.Id.restrictedIcon);
				RestrictedTextView = (TextView)view.FindViewById(Resource.Id.restrictedTextview);
				PurchaseButton = (AppCompatButton)view.FindViewById(Resource.Id.purchaseButton);
				PurchaseButton.Visibility = ViewStates.Gone;

				PurchaseButton.Click += PurchaseButtonOnClick;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Event

		private void PurchaseButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (UserDetails.IsLogin)
				{
					var dialog = new MaterialAlertDialogBuilder(Context);
					dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
					dialog.SetMessage(Context.GetText(Resource.String.Lbl_RentVideo));
					dialog.SetPositiveButton(Context.GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
					{
						try
						{
							var type = PurchaseButton.Tag?.ToString();
							string price = "";
							switch (type)
							{
								case "RentVideo":
									price = VideoObject.RentPrice;
									break;
								case "purchaseVideo":
									price = VideoObject.SellVideo;
									break;
								case "Subscriber":
									price = VideoObject.Owner?.OwnerClass?.SubscriberPrice;
									break;
							}
							 
							if (await AppTools.CheckWallet(Convert.ToDouble(price)))
							{
								if (Methods.CheckConnectivity())
								{
									if (type == "RentVideo")
									{ 
										var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("rent", VideoObject.Id);
										if (apiStatus == 200)
										{
											if (respond is MessageObject result)
											{
												Console.WriteLine(result.Message);

												Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
												if (VideoControl?.VideoData != null)
												{
													VideoControl.VideoData.RentPrice = "0"; 
													VideoControl.LoadDataVideo(VideoControl.VideoData);
												}
											}
										}
										else Methods.DisplayAndHudErrorResult(Activity, respond);
									}
									else if (type == "purchaseVideo")
									{
										var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("buy", VideoObject.Id);
										if (apiStatus == 200)
										{
											if (respond is MessageObject result)
											{
												Console.WriteLine(result.Message);

												Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
												if (VideoControl?.VideoData != null)
												{
													VideoControl.VideoData.SellVideo = "0";
													VideoControl.LoadDataVideo(VideoControl.VideoData);
												} 
											}
										}
										else Methods.DisplayAndHudErrorResult(Activity, respond);
									}
									else if (type == "Subscriber")
									{
										var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("subscribe", VideoObject.Owner?.OwnerClass.Id);
										if (apiStatus == 200)
										{
											if (respond is MessageObject result)
											{
												Console.WriteLine(result.Message);

												Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
												// SetSubscribeChannelWithPaid();

												if (VideoControl?.VideoData?.Owner != null)
												{
													VideoControl.VideoData.Owner.Value.OwnerClass.AmISubscribed = "1";
													VideoControl.VideoData.Owner.Value.OwnerClass.SubscriberPrice = "0";
													VideoControl.LoadDataVideo(VideoControl.VideoData);
												} 
											}
										}
										else Methods.DisplayAndHudErrorResult(Activity, respond);
									}
								}
								else
									Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
							}
							else
							{
								var dialogBuilder = new MaterialAlertDialogBuilder(Context);
								dialogBuilder.SetTitle(Context.GetText(Resource.String.Lbl_Wallet));
								dialogBuilder.SetMessage(Context.GetText(Resource.String.Lbl_Error_NoWallet));
								dialogBuilder.SetPositiveButton(Context.GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
								{
									try
									{
										Context.StartActivity(new Intent(Context, typeof(WalletActivity)));
									}
									catch (Exception exception)
									{
										Methods.DisplayReportResultTrack(exception);
									}
								});
								dialogBuilder.SetNegativeButton(Context.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

								dialogBuilder.Show();
							}
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
						}
					});
					dialog.SetNegativeButton(Context.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

					dialog.Show();
				}
				else
				{
					PopupDialogController dialog = new PopupDialogController(Activity, VideoObject, "Login");
					dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_Paid), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data

		public void HideRestrictedInfo(bool hide)
		{
			try
			{
				if (RestrictedTextView != null)
					RestrictedTextView.Visibility = hide ? ViewStates.Gone : ViewStates.Visible;

				if (RestrictedIcon != null)
					RestrictedIcon.Visibility = hide ? ViewStates.Gone : ViewStates.Visible;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void UpdateRestrictionData(string strtext, string imageUrl, VideoDataObject videoObject)
		{
			try
			{
				RestrictionText = strtext;
				ImageUrl = imageUrl;
				VideoObject = videoObject;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void LoadRestriction(string strtext, string imageUrl, VideoDataObject videoObject)
		{
			try
			{
				HideRestrictedInfo(false);
				VideoObject = videoObject;
				switch (strtext)
				{
					case "AgeRestriction":
						{
							RestrictedIcon.SetImageResource(Resource.Drawable.icon_18plus);
							RestrictedTextView.Text = GetText(Resource.String.Lbl_AgeRestricted);
							if (!string.IsNullOrEmpty(imageUrl))
								GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
							break;
						}
					case "GeoRestriction":
						{
							RestrictedIcon.SetImageResource(Resource.Drawable.ic_GeoRestict);
							RestrictedTextView.Text = GetText(Resource.String.Lbl_GEORestricted);
							if (!string.IsNullOrEmpty(imageUrl))
								GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
							break;
						}
					case "purchaseVideo":
						{
							RestrictedIcon.SetImageResource(Resource.Drawable.icon_dollars_vector);
							RestrictedTextView.Text = GetText(Resource.String.Lbl_purchaseVideo);
							if (!string.IsNullOrEmpty(imageUrl))
								GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

							var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
							var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
							Console.WriteLine(currency);
							PurchaseButton.Visibility = ViewStates.Visible;
							PurchaseButton.Text = GetText(Resource.String.Lbl_Purchase) + " " + currencyIcon + videoObject.SellVideo;
							PurchaseButton.Tag = "purchaseVideo";
							break;
						}
					case "RentVideo":
						{
							RestrictedIcon.SetImageResource(Resource.Drawable.icon_dollars_vector);
							RestrictedTextView.Text = GetText(Resource.String.Lbl_RentVideo);
							if (!string.IsNullOrEmpty(imageUrl))
								GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

							var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
							var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
							Console.WriteLine(currency);
							PurchaseButton.Visibility = ViewStates.Visible;
							PurchaseButton.Text = GetText(Resource.String.Lbl_Rent) + " " + currencyIcon + videoObject.RentPrice;
							PurchaseButton.Tag = "RentVideo";
							break;
						}
					// SubscribeChannel
					case "Subscriber":
						{
							RestrictedIcon.SetImageResource(Resource.Drawable.icon_dollars_vector);

							var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
							var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
							Console.WriteLine(currency);

							RestrictedTextView.Text = Activity.GetText(Resource.String.Lbl_SubscribeFor) + " " + currencyIcon + videoObject.Owner?.OwnerClass?.SubscriberPrice + " " + Activity.GetText(Resource.String.Lbl_AndUnlockAllTheVideos);
							if (!string.IsNullOrEmpty(imageUrl))
								GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

							PurchaseButton.Visibility = ViewStates.Visible;
							PurchaseButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + videoObject.Owner?.OwnerClass?.SubscriberPrice;
							PurchaseButton.Tag = "Subscriber";
							break;
						}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion
	}
}