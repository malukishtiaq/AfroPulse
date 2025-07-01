using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;
using System.Collections.ObjectModel;
using Console = System.Console;

namespace PlayTube.Activities.Upgrade.Adapters
{
	public class UpgradeGoProClass
	{
		public int Id { get; set; }
		public string HexColor { get; set; }
		public string BtnText { get; set; }
		public string PlanTime { get; set; }
		public string PlanPrice { get; set; }
		public int ImageResource { get; set; }
	}

	public class UpgradeGoProAdapter : RecyclerView.Adapter
	{
		#region Variables Basic

		public event EventHandler<UpgradeGoProAdapterClickEventArgs> UpgradeButtonItemClick;
		public event EventHandler<UpgradeGoProAdapterClickEventArgs> ItemClick;
		public event EventHandler<UpgradeGoProAdapterClickEventArgs> ItemLongClick;

		private readonly Activity ActivityContext;
		private TextDecorator WoTextDecorator { get; set; }
		public readonly ObservableCollection<UpgradeGoProClass> PlansList = new ObservableCollection<UpgradeGoProClass>();

		#endregion

		public UpgradeGoProAdapter(Activity context)
		{
			try
			{
				ActivityContext = context;
				WoTextDecorator = new TextDecorator();

				PlansList.Add(new UpgradeGoProClass { Id = 1, HexColor = AppSettings.MainColor, BtnText = ActivityContext.GetText(Resource.String.Lbl_StayFree), PlanPrice = "0", PlanTime = ActivityContext.GetText(Resource.String.Lbl_FreeMember), ImageResource = Resource.Drawable.ic_star });
				PlansList.Add(new UpgradeGoProClass { Id = 2, HexColor = "#9135fa", BtnText = ActivityContext.GetText(Resource.String.Lbl_upgrade_now), PlanPrice = ListUtils.MySettingsList?.ProPkgPrice, PlanTime = ActivityContext.GetText(Resource.String.Lbl_ProMember), ImageResource = Resource.Drawable.gopro_medal });
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override int ItemCount => PlansList?.Count ?? 0;

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> Style_GiftView
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_GoPro_PricessView, parent, false);
				UpgradePlansViewHolder vh = new UpgradePlansViewHolder(itemView, UpgradeButtonClick, Click, LongClick);
				return vh;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		// Replace the contents of a view (invoked by the layout manager)
		public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
		{
			try
			{
				if (viewHolder is UpgradePlansViewHolder holder)
				{
					UpgradeGoProClass item = PlansList[position];
					if (item != null)
					{
						if (AppTools.IsTabDark())
						{
							holder.MainLayout.SetBackgroundResource(Resource.Drawable.ShadowLinerLayoutDark);
							holder.RelativeLayout.SetBackgroundResource(Resource.Drawable.price_gopro_item_style_dark);
						}

						holder.PlanImg.SetImageResource(item.ImageResource);
						holder.PlanImg.SetColorFilter(Color.ParseColor(item.HexColor));

						var idCurrency = ListUtils.MySettingsList?.PaypalCurrency ?? "USD";
						var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
						Console.WriteLine(currency);

						holder.PriceText.Text = currencyIcon + item.PlanPrice;

						//holder.PlanText.Text = item.PlanText;
						holder.PerText.Text = item.PlanTime;
						holder.UpgradeButton.Text = item.BtnText;

						//holder.PlanText.SetTextColor(Color.ParseColor(item.HexColor));
						holder.PriceText.SetTextColor(Color.ParseColor(item.HexColor));
						holder.UpgradeButton.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(item.HexColor));

						Typeface font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
						string name = "go_pro_array_" + item.Id;
						int? resourceId = ActivityContext.Resources?.GetIdentifier(name, "array", ActivityContext.ApplicationInfo.PackageName);
						if (resourceId == 0)
						{
							return;
						}

						string[] planArray = ActivityContext.Resources?.GetStringArray(resourceId.Value);
						if (planArray != null)
						{
							foreach (string options in planArray)
							{
								if (!string.IsNullOrEmpty(options))
								{
									TextView text = new TextView(ActivityContext)
									{
										Text = options,
										TextSize = 13
									};
									text.SetTextColor(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#444444"));
									text.Gravity = GravityFlags.CenterHorizontal;
									text.SetTypeface(font, TypefaceStyle.Normal);
									WoTextDecorator.Content = options;
									WoTextDecorator.DecoratedContent = new Android.Text.SpannableString(options);
									//WoTextDecorator.SetTextColor(IonIconsFonts.Checkmark, "#43a735");
									//WoTextDecorator.SetTextColor(IonIconsFonts.Close, "#e13c4c");

									LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);//height and width are inpixel
									layoutParams.SetMargins(0, 30, 0, 5);

									text.LayoutParameters = layoutParams;
									holder.OptionLinerLayout.AddView(text);
									WoTextDecorator.Build(text, WoTextDecorator.DecoratedContent);
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public UpgradeGoProClass GetItem(int position)
		{
			return PlansList[position];
		}

		public override long GetItemId(int position)
		{
			try
			{
				return position;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return 0;
			}
		}

		public override int GetItemViewType(int position)
		{
			try
			{
				return position;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return 0;
			}
		}

		private void UpgradeButtonClick(UpgradeGoProAdapterClickEventArgs args)
		{
			UpgradeButtonItemClick?.Invoke(this, args);
		}

		private void Click(UpgradeGoProAdapterClickEventArgs args)
		{
			ItemClick?.Invoke(this, args);
		}

		private void LongClick(UpgradeGoProAdapterClickEventArgs args)
		{
			ItemLongClick?.Invoke(this, args);
		}
	}

	public class UpgradePlansViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public LinearLayout MainLayout { get; private set; }
		public ImageView PlanImg { get; private set; }
		public TextView PriceText { get; private set; }
		public TextView PerText { get; private set; }
		public AppCompatButton UpgradeButton { get; private set; }
		//public TextView PlanText { get; private set; }
		public LinearLayout OptionLinerLayout { get; private set; }
		public View MainView { get; private set; }
		public RelativeLayout RelativeLayout { get; private set; }

		#endregion

		public UpgradePlansViewHolder(View itemView, Action<UpgradeGoProAdapterClickEventArgs> upgradeButtonClickListener, Action<UpgradeGoProAdapterClickEventArgs> clickListener, Action<UpgradeGoProAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				MainLayout = MainView.FindViewById<LinearLayout>(Resource.Id.mainLayout);
				PlanImg = MainView.FindViewById<ImageView>(Resource.Id.iv1);
				PriceText = MainView.FindViewById<TextView>(Resource.Id.priceTextView);
				PerText = MainView.FindViewById<TextView>(Resource.Id.PerText);
				//PlanText = MainView.FindViewById<TextView>(Resource.Id.PlanText);
				OptionLinerLayout = MainView.FindViewById<LinearLayout>(Resource.Id.OptionLinerLayout);
				UpgradeButton = MainView.FindViewById<AppCompatButton>(Resource.Id.UpgradeButton);
				RelativeLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);

				UpgradeButton.Click += (sender, e) => upgradeButtonClickListener(new UpgradeGoProAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.Click += (sender, e) => clickListener(new UpgradeGoProAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new UpgradeGoProAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}

	public class UpgradeGoProAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}