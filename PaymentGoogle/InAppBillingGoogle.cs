using Android.BillingClient.Api;
using System.Collections.Generic;

namespace PlayTube.PaymentGoogle
{
	public static class InAppBillingGoogle
	{
		public const string Membership = "membership";
		public const string RentVideo = "rentvideo";

		public static readonly List<QueryProductDetailsParams.Product> ListProductSku = new List<QueryProductDetailsParams.Product> // ID Product
        {
            //All products should be of the same product type.
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Membership).SetProductType(BillingClient.IProductType.Subs).Build(),
			QueryProductDetailsParams.Product.NewBuilder().SetProductId(RentVideo).SetProductType(BillingClient.IProductType.Subs).Build(),
		};
	}
}