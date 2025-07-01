using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Com.Razorpay;
using Google.Android.Material.Dialog;
using InAppBilling.Lib;
using IyziPay;
using IyziPay.Lib.Model;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using PlayTube.PaymentGoogle;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Payment;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using SecurionPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayTube.Payment.Utils
{
	[Activity]
	public class PaymentBaseActivity : BaseActivity, IBillingPaymentListener, IPaymentResultWithDataListener, ISecurionPayPaymentListener, IIyziPayPaymentListener, IDialogInputCallBack
	{
		public BillingSupport BillingSupport;
		public InitPayPalPayment InitPayPalPayment;
		public InitCashFreePayment CashFreePayment;
		public InitRazorPayPayment InitRazorPay;
		public InitSecurionPayPayment SecurionPayPayment;
		public InitIyziPayPayment IyziPayPayment;
		public InitPayStackPayment PayStackPayment;
		public InitPaySeraPayment PaySeraPayment;
		public InitAamarPayPayment AamarPayPayment;

		public string Price = "0";
		public readonly string Type = "";

		public void InitBuy()
		{
			try
			{
				if (AppSettings.ShowInAppBilling && InitializePlayTube.IsExtended)
					BillingSupport = new BillingSupport(this, AppSettings.TripleDesAppServiceProvider, InAppBillingGoogle.ListProductSku, this);

				if (AppSettings.ShowPaypal)
					InitPayPalPayment ??= new InitPayPalPayment(this);

				if (AppSettings.ShowRazorPay)
					InitRazorPay ??= new InitRazorPayPayment(this);

				if (AppSettings.ShowPayStack)
					PayStackPayment ??= new InitPayStackPayment(this);

				if (AppSettings.ShowCashFree)
					CashFreePayment ??= new InitCashFreePayment(this);

				if (AppSettings.ShowPaySera)
					PaySeraPayment ??= new InitPaySeraPayment(this);

				if (AppSettings.ShowSecurionPay)
					SecurionPayPayment ??= new InitSecurionPayPayment(this, this, ListUtils.MySettingsList?.SecurionpayPublicKey);

				if (AppSettings.ShowAamarPay)
					AamarPayPayment ??= new InitAamarPayPayment(this);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#region Billing

		public void OnPaymentSuccess(IList<Purchase> result)
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
					return;
				}

				//if (PayType == "membership")
				//{ 
				//    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SetProApi("Google InApp") }); 
				//}
				//else
				//{
				//    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SetCredit("Google InApp") });
				//}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void GetPurchase(IList<Purchase> result)
		{

		}

		#endregion

		#region RazorPay

		public void OnPaymentError(int code, string response, PaymentData p2)
		{
			try
			{
				Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
				Toast.MakeText(this, "Payment failed: " + response, ToastLength.Long)?.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public async void OnPaymentSuccess(string razorpayPaymentId, PaymentData p1)
		{
			try
			{
				Console.WriteLine("razorpay : Payment Successful:" + razorpayPaymentId);

				if (!string.IsNullOrEmpty(razorpayPaymentId) && Methods.CheckConnectivity())
				{
					var priceInt = Convert.ToInt32(Price) * 100;

					var (apiStatus, respond) = await RequestsAsync.Payment.RazorPayAsync(razorpayPaymentId, "wallet", priceInt.ToString());
					if (apiStatus == 200)
					{
						WalletActivity.GetInstance().TxtAmount.Text = string.Empty;
						Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
					}
					else
						Methods.DisplayReportResult(this, respond);
				}
				else if (!string.IsNullOrEmpty(razorpayPaymentId))
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region CashFree

		private EditText TxtName, TxtEmail, TxtPhone;
		public void OpenCashFreeDialog()
		{
			try
			{
				var dialog = new MaterialAlertDialogBuilder(this);

				dialog.SetTitle(Resource.String.Lbl_CashFree);

				View view = LayoutInflater.Inflate(Resource.Layout.CashFreePaymentLayout, null);
				dialog.SetView(view);
				dialog.SetPositiveButton(GetText(Resource.String.Lbl_PayNow), async (o, args) =>
				{
					try
					{
						if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
						{
							Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short)?.Show();
							return;
						}

						var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
						switch (check)
						{
							case false:
								Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
								return;
						}

						if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
						{
							Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
							return;
						}

						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

						await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text);
					}
					catch (Exception e)
					{
						Methods.DisplayReportResultTrack(e);
					}
				});
				dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				var iconName = view.FindViewById<TextView>(Resource.Id.IconName);
				TxtName = view.FindViewById<EditText>(Resource.Id.NameEditText);

				var iconEmail = view.FindViewById<TextView>(Resource.Id.IconEmail);
				TxtEmail = view.FindViewById<EditText>(Resource.Id.EmailEditText);

				var iconPhone = view.FindViewById<TextView>(Resource.Id.IconPhone);
				TxtPhone = view.FindViewById<EditText>(Resource.Id.PhoneEditText);

				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconName, FontAwesomeIcon.User);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconEmail, FontAwesomeIcon.PaperPlane);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconPhone, FontAwesomeIcon.Mobile);

				Methods.SetColorEditText(TxtName, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtEmail, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtPhone, AppTools.IsTabDark() ? Color.White : Color.Black);

				var local = ListUtils.MyChannelList?.FirstOrDefault();
				if (local != null)
				{
					TxtName.Text = AppTools.GetNameFinal(local);
					TxtEmail.Text = local.Email;
					TxtPhone.Text = local.PhoneNumber;
				}

				dialog.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private async Task CashFree(string name, string email, string phone)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					var keyValues = new Dictionary<string, string>
					{
						{"name", name},
						{"phone", phone},
						{"email", email},
						{"amount", Price},
					};

					var (apiStatus, respond) = await RequestsAsync.Payment.InitializeCashFreeAsync("wallet", AppSettings.CashFreeCurrency, ListUtils.MySettingsList?.CashfreeSecretKey ?? "", ListUtils.MySettingsList?.CashfreeMode, keyValues);
					switch (apiStatus)
					{
						case 200:
							{
								switch (respond)
								{
									case CashFreeObject result:
										CashFreePayment ??= new InitCashFreePayment(this);
										CashFreePayment.DisplayCashFreePayment(result, Price);
										break;
								}

								break;
							}
						default:
							Methods.DisplayReportResult(this, respond);
							break;
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region PayStack

		public void OpenPayStackDialog()
		{
			try
			{
				var dialogBuilder = new MaterialAlertDialogBuilder(this);
				dialogBuilder.SetTitle(Resource.String.Lbl_PayStack);

				EditText input = new EditText(this);
				input.SetHint(Resource.String.Lbl_Email);
				input.InputType = InputTypes.TextVariationEmailAddress;
				LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
				input.LayoutParameters = lp;

				dialogBuilder.SetView(input);

				dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_PayNow), new MaterialDialogUtils(input, this));
				dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
				dialogBuilder.Show();

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private async Task PayStack(string email)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					var priceInt = Convert.ToInt32(Price) * 100;

					var keyValues = new Dictionary<string, string>
					{
						{"email", email},
						{"amount", priceInt.ToString()},
					};

					var (apiStatus, respond) = await RequestsAsync.Payment.InitializePayStackAsync("wallet", keyValues);
					switch (apiStatus)
					{
						case 200:
							{
								switch (respond)
								{
									case InitializePaymentObject result:
										PayStackPayment ??= new InitPayStackPayment(this);
										PayStackPayment.DisplayPayStackPayment(result.Url, priceInt.ToString());
										break;
								}

								break;
							}
						default:
							Methods.DisplayReportResult(this, respond);
							break;
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region PaySera

		public async Task PaySera()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					var keyValues = new Dictionary<string, string>
					{
						{"amount", Price},
					};

					var (apiStatus, respond) = await RequestsAsync.Payment.InitializePaySeraAsync("wallet", keyValues);
					switch (apiStatus)
					{
						case 200:
							{
								switch (respond)
								{
									case InitializePaymentObject result:
										PaySeraPayment ??= new InitPaySeraPayment(this);
										PaySeraPayment.DisplayPaySeraPayment(result.Url, Price);
										break;
								}

								break;
							}
						default:
							Methods.DisplayReportResult(this, respond);
							break;
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region SecurionPay

		public async Task OpenSecurionPay()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					var (apiStatus, respond) = await RequestsAsync.Payment.InitializeSecurionPayAsync("securionpay_token", Price);
					switch (apiStatus)
					{
						case 200:
							{
								switch (respond)
								{
									case InitializeSecurionPayObject result:
										SecurionPayPayment ??= new InitSecurionPayPayment(this, this, ListUtils.MySettingsList?.SecurionpayPublicKey);
										SecurionPayPayment.DisplaySecurionPayPayment(result.Token, Price, AppTools.IsTabDark() ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
										break;
								}

								break;
							}
						default:
							Methods.DisplayReportResult(this, respond);
							break;
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private async Task SecurionPay(string request, string charge)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					var (apiStatus, respond) = await RequestsAsync.Payment.SecurionPayAsync(request, charge);
					switch (apiStatus)
					{
						case 200:
							Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

							break;
						default:
							Methods.DisplayReportResult(this, respond);
							break;
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnPaymentError(string error)
		{
			try
			{

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnPaymentSuccess(SecurionPayResult result)
		{
			try
			{
				if (!string.IsNullOrEmpty(result?.Charge?.Id))
				{
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SecurionPay("securionpay_handle", JsonConvert.SerializeObject(result.Charge)) });
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region IyziPay

		public void IyziPay()
		{
			try
			{
				IyziPayPaymentObject request = new IyziPayPaymentObject()
				{
					Locale = Locale.TR,

					Id = ListUtils.MySettingsList.IyzipayBuyerId,
					Name = ListUtils.MySettingsList.IyzipayBuyerName,
					Surname = ListUtils.MySettingsList.IyzipayBuyerSurname,
					GsmNumber = ListUtils.MySettingsList.IyzipayBuyerGsmNumber,
					Email = ListUtils.MySettingsList.IyzipayBuyerEmail,
					IdentityNumber = ListUtils.MySettingsList.IyzipayIdentityNumber,
					Address = ListUtils.MySettingsList.IyzipayAddress,
					City = ListUtils.MySettingsList.IyzipayCity,
					Country = ListUtils.MySettingsList.IyzipayCountry,
					Zip = ListUtils.MySettingsList.IyzipayZip,

					Price = Price,
					Currency = Currency.TRY,
					CallbackUrl = InitializePlayTube.WebsiteUrl + "/requests.php?f=iyzipay&s=success&amount=" + Price,

					ApiKey = ListUtils.MySettingsList.IyzipayKey,
					SecretKey = ListUtils.MySettingsList.IyzipaySecretKey,
					BaseUrl = ListUtils.MySettingsList.IyzipayMode == "0" ? "https://merchant.iyzipay.com/" : "https://sandbox-api.iyzipay.com/"
				};

				IyziPayPayment ??= new InitIyziPayPayment(this, this, request);
				IyziPayPayment.DisplayIyziPayPayment(Price, AppTools.IsTabDark() ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
				//string Token = IyziPayPayment.CheckoutFormInitialize?.Token; 
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnIyziPayPaymentSuccess(CheckoutFormInitialize result)
		{
			try
			{
				if (!string.IsNullOrEmpty(result?.Token))
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => IyziPay(result?.Token) });
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private async Task IyziPay(string token)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					var (apiStatus, respond) = await RequestsAsync.Payment.IyziPayAsync("wallet", token, Price);
					switch (apiStatus)
					{
						case 200:
							Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

							IyziPayPayment.StopIyziPay();
							break;
						default:
							Methods.DisplayReportResult(this, respond);
							break;
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region YooMoney

		//private async Task YooMoney()
		//{
		//    try
		//    {
		//        if (Methods.CheckConnectivity())
		//        {
		//            var (apiStatus, respond) = await RequestsAsync.Payment.InitializeYooMoneyAsync("create_yoomoney", Price);
		//            switch (apiStatus)
		//            {
		//                case 200:
		//                {
		//                    switch (respond)
		//                    {
		//                        case InitializeYooMoneyObject result:
		//                           // OpenIntentYooMoney();
		//                            break;
		//                    } 
		//                    break;
		//                }
		//                default:
		//                    Methods.DisplayReportResult(this, respond);
		//                    break;
		//            }
		//        }
		//        else
		//        {
		//            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
		//        }
		//    }
		//    catch (Exception e)
		//    {
		//        Methods.DisplayReportResultTrack(e);
		//    }
		//}

		private void OpenIntentYooMoney()
		{
			try
			{
				//var amount = new Amount(BigDecimal.ValueOf(10.0), Currency.GetInstance("RUB"));
				//var title = "product name";
				//var subtitle = "product description";
				//var clientApplicationKey = "";
				//var shopId = "";
				//var savePaymentMethod = SavePaymentMethod.Off;
				//var paymentMethodTypes = new List<PaymentMethodType>() { PaymentMethodType.GooglePay, PaymentMethodType.BankCard, PaymentMethodType.Sberbank, PaymentMethodType.YooMoney };
				//var gatewayId = "";
				//var customReturnUrl = "test_redirect_url";
				//var userPhoneNumber = "test_phone_number";
				//var googlePayParameters = new GooglePayParameters();
				//var authCenterClientId = "";

				//var paymentParameters = new PaymentParameters(amount, title, subtitle, clientApplicationKey, shopId, savePaymentMethod, paymentMethodTypes, gatewayId, customReturnUrl, userPhoneNumber, googlePayParameters, authCenterClientId);
				//var intent = RU.Yoomoney.Sdk.Kassa.Payments.Checkout.CreateTokenizeIntent(this, paymentParameters, new TestParameters(true));
				//StartActivityForResult(intent, 2325);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Api

		public async Task SetProApi()
		{
			if (Methods.CheckConnectivity())
			{
				var (apiStatus, respond) = await RequestsAsync.Global.UpgradeAsync();
				if (apiStatus == 200)
				{
					var dataUser = ListUtils.MyChannelList?.FirstOrDefault();
					if (dataUser != null)
					{
						dataUser.IsPro = "1";

						var sqlEntity = new SqLiteDatabase();
						sqlEntity.InsertOrUpdate_DataMyChannel(dataUser);
					}

					Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
					Finish();
				}
				else Methods.DisplayReportResult(this, respond);
			}
			else
			{
				Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
			}
		}


		public async void TopWallet()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					//var (apiStatus, respond) = await RequestsAsync.Payment.TopWalletAsync(Price);
					//if (apiStatus == 200)
					//{
					//    if (respond is MessageObject result)
					//    {
					//        //wael add after update api
					//        WalletActivity.GetInstance()?.Get_Data_User();
					//        //var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
					//        //if (dataUser != null)
					//        //{
					//        //  dataUser.Balance = creditObject.Balance;

					//        //  var sqlEntity = new SqLiteDatabase();
					//        //  sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
					//        //}

					//        //if (WalletActivity.GetInstance().WalletNumber != null)
					//        //    WalletActivity.GetInstance().WalletNumber.Text = result.Balance.Replace(".00", "");

					//        Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
					//        AndHUD.Shared.Dismiss();

					//        Finish();
					//    }
					//}
					//else Methods.DisplayAndHudErrorResult(this, respond);
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
					AndHUD.Shared.Dismiss();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				AndHUD.Shared.Dismiss();
			}
		}

		#endregion

		#region MaterialDialog

		public async void OnInput(IDialogInterface dialog, string input)
		{
			try
			{
				if (!string.IsNullOrEmpty(input))
				{
					if (input.Length <= 0) return;
					var check = Methods.FunString.IsEmailValid(input.Replace(" ", ""));
					if (!check)
					{
						Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
						return;
					}

					Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();

					await PayStack(input);
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