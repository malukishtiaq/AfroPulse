using Android.App;
using Android.Widget;
using AndroidHUD;
using Com.Cashfree.PG.Api;
using Com.Cashfree.PG.Core.Api;
using Com.Cashfree.PG.Core.Api.Callback;
using Com.Cashfree.PG.Core.Api.Utils;
using Com.Cashfree.PG.UI.Api;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Payment;
using PlayTube.PlayTubeClient.RestCalls;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace PlayTube.Payment
{
	public class InitCashFreePayment : Object, ICFCheckoutResponseCallback
	{
		private readonly Activity ActivityContext;
		private string Price;
		private CashFreeObject CashFreeObject;

		public InitCashFreePayment(Activity context)
		{
			try
			{
				ActivityContext = context;

				CFPaymentGatewayService.Initialize(context); // Application Context.
				AnalyticsUtil.SendPaymentEventsToBackend(); // required for error reporting.

				CFPaymentGatewayService.Instance?.SetCheckoutCallback(this);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void DisplayCashFreePayment(CashFreeObject cashFreeObject, string price)
		{
			ActivityContext.RunOnUiThread(() =>
			{
				try
				{
					CashFreeObject = cashFreeObject;
					Price = price;

					CFSession.Environment cfEnvironment = ListUtils.MySettingsList?.CashfreeMode switch
					{
						"SandBox" => CFSession.Environment.Sandbox,
						"Live" => CFSession.Environment.Production,
						_ => CFSession.Environment.Sandbox
					};

					CFSession cfSession = new CFSession.CFSessionBuilder()
						.SetEnvironment(cfEnvironment)
						.SetPaymentSessionID(CashFreeObject.OrderLinkObject.PaymentSessionId)
						.SetOrderId(CashFreeObject.OrderId)
						.Build();

					//CFPaymentComponent cfPaymentComponent = new CFPaymentComponent.CFPaymentComponentBuilder()
					//    .Add(CFPaymentComponent.CFPaymentModes.Card)
					//    .Add(CFPaymentComponent.CFPaymentModes.Upi)
					//    .Add(CFPaymentComponent.CFPaymentModes.Wallet)
					//    .Build();

					CFTheme cfTheme = new CFTheme.CFThemeBuilder()
						.SetNavigationBarBackgroundColor(AppSettings.MainColor)
						.SetNavigationBarTextColor("#ffffff")
						.SetButtonBackgroundColor(AppSettings.MainColor)
						.SetButtonTextColor("#ffffff")
						.SetPrimaryTextColor("#000000")
						.SetSecondaryTextColor("#000000")
						.Build();

					CFDropCheckoutPayment cfDropCheckoutPayment = new CFDropCheckoutPayment.CFDropCheckoutPaymentBuilder()
						.SetSession(cfSession)
						//By default all modes are enabled. If you want to restrict the payment modes uncomment the next line
						//.SetCFUIPaymentModes(cfPaymentComponent)
						.SetCFNativeCheckoutUITheme(cfTheme)
						.Build();

					CFPaymentGatewayService gatewayService = CFPaymentGatewayService.Instance;
					gatewayService.DoPayment(ActivityContext, cfDropCheckoutPayment);
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			});
		}

		public void OnPaymentFailure(CFErrorResponse cfErrorResponse, string orderId)
		{
			try
			{
				//Error  
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public async void OnPaymentVerify(string orderId)
		{
			try
			{
				//verifyPayment triggered
				if (Methods.CheckConnectivity())
				{
					var (apiStatus, respond) = await RequestsAsync.Payment.CashFreeAsync("wallet_paid", orderId);
					switch (apiStatus)
					{
						case 200:
							AndHUD.Shared.Dismiss();

							Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Short)?.Show();
							break;
						default:
							Methods.DisplayAndHudErrorResult(ActivityContext, respond);
							break;
					}
				}
				else
				{
					Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}