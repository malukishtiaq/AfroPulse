using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.AppBar;
using Google.Android.Material.Dialog;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using PlayTube.Activities.Channel.Tab;
using PlayTube.Activities.Chat;
using PlayTube.Activities.Search;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Activities.Tabbes;
using PlayTube.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppCompatButton = AndroidX.AppCompat.Widget.AppCompatButton;
using Exception = System.Exception;
using Methods = PlayTube.Helpers.Utils.Methods;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Channel
{
	public class UserChannelFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy
	{
		#region Variables Basic

		public static UserChannelFragment Instance;
		private ChPlayListFragment PlayListFragment;
		private ChVideosFragment VideosFragment;
		private ChShortsFragment ShortsFragment;
		public ChActivitiesFragment ActivitiesFragment;
		private ChAboutFragment AboutFragment;
		private AppBarLayout AppBarLayout;
		private TabLayout Tabs;
		private ViewPager2 ViewPagerView;
		private Toolbar MainToolbar;
		private ImageView ImageChannel, ImageCoverChannel, IconMesseges, IconSearch;
		private CollapsingToolbarLayout CollapsingToolbar;
		private TextView ChannelNameText;
		public TextView VideoCountText;
		private ImageView ChannelVerifiedText;
		private string IdChannel = "";
		private TabbedMainActivity GlobalContext;
		public AppCompatButton SubscribeChannelButton;
		private TextView TxtSubscribeCount;
		public UserDataObject UserData;

		private MainTabAdapter Adapter;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			GlobalContext = (TabbedMainActivity)Activity;
			HasOptionsMenu = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				View view = inflater?.Inflate(Resource.Layout.UserChannel_Layout, container, false);
				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				base.OnViewCreated(view, savedInstanceState);

				Instance = this;

				if (!string.IsNullOrEmpty(Arguments?.GetString("Object")))
				{
					UserData = JsonConvert.DeserializeObject<UserDataObject>(Arguments?.GetString("Object") ?? "");
					if (UserData != null)
					{
						IdChannel = UserData.Id;
					}
				}

				//Get Value And Set Toolbar
				InitComponent(view);
				InitToolbar(view);
				SetTap();

				SubscribeChannelButton.Click += SubscribeChannelButtonClick;
				IconMesseges.Click += IconMessegesOnClick;
				IconSearch.Click += IconSearchOnClick;

				GetDataUser();

				AdsGoogle.Ad_RewardedVideo(Activity);
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

		public override void OnDestroyView()
		{
			try
			{
				Instance = null;
				base.OnDestroyView();
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
				IconMesseges = view.FindViewById<ImageView>(Resource.Id.Messeges_icon);
				IconSearch = view.FindViewById<ImageView>(Resource.Id.Search_icon);

				ImageCoverChannel = view.FindViewById<ImageView>(Resource.Id.Imagevideo);
				ImageChannel = view.FindViewById<ImageView>(Resource.Id.ChannelImage);
				CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsingToolbar);
				ChannelNameText = view.FindViewById<TextView>(Resource.Id.ChannelName);
				ChannelVerifiedText = view.FindViewById<ImageView>(Resource.Id.ChannelVerifiedText);
				SubscribeChannelButton = view.FindViewById<AppCompatButton>(Resource.Id.SubcribeButton);

				TxtSubscribeCount = view.FindViewById<TextView>(Resource.Id.subcriberscount);

				Tabs = view.FindViewById<TabLayout>(Resource.Id.channeltabs);
				ViewPagerView = view.FindViewById<ViewPager2>(Resource.Id.Channelviewpager);
				AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
				AppBarLayout.SetExpanded(true);

				if (!UserDetails.IsLogin)
					IconMesseges.Visibility = ViewStates.Gone;

				ChannelVerifiedText.Visibility = ViewStates.Gone;
				SubscribeChannelButton.Tag = "Subscribe";

				VideoCountText = view.FindViewById<TextView>(Resource.Id.videocount);

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar(View view)
		{
			try
			{
				MainToolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
				GlobalContext.SetToolBar(MainToolbar, " ");

				var icon = AppCompatResources.GetDrawable(Context, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
				icon?.SetTint(Color.White);
				GlobalContext.SupportActionBar.SetHomeAsUpIndicator(icon);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetTap()
		{
			try
			{
				ViewPagerView.OffscreenPageLimit = 3;
				SetUpViewPager(ViewPagerView);
				//Tabs.SetTabTextColors(Color.White, Color.ParseColor(AppSettings.MainColor));
				new TabLayoutMediator(Tabs, ViewPagerView, this).Attach();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void IconSearchOnClick(object sender, EventArgs e)
		{
			try
			{
				SearchFragment searchFragment = new SearchFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(searchFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void IconMessegesOnClick(object sender, EventArgs e)
		{
			try
			{
				UserDataObject item = UserData;
				if (item != null)
				{
					Intent intent = new Intent(Activity, typeof(MessagesBoxActivity));
					intent.PutExtra("UserId", IdChannel);
					intent.PutExtra("TypeChat", "Owner");
					intent.PutExtra("UserItem", JsonConvert.SerializeObject(item));
					Activity.StartActivity(intent);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SubscribeChannelButtonClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						if (!string.IsNullOrEmpty(UserData.SubscriberPrice) && UserData.SubscriberPrice != "0")
						{
							if (SubscribeChannelButton.Tag?.ToString() == "PaidSubscribe")
							{
								//This channel is paid, You must pay to subscribe

								var dialog = new MaterialAlertDialogBuilder(Context);
								dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
								dialog.SetMessage(Context.GetText(Resource.String.Lbl_ChannelIsPaid));
								dialog.SetPositiveButton(Context.GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
								{
									try
									{
										if (await AppTools.CheckWallet(Convert.ToDouble(UserData.SubscriberPrice)))
										{
											if (Methods.CheckConnectivity())
											{
												var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("subscribe", UserData.Id);
												if (apiStatus == 200)
												{
													if (respond is MessageObject result)
													{
														Console.WriteLine(result.Message);

														Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
														SetSubscribeChannelWithPaid();
													}
												}
												else Methods.DisplayAndHudErrorResult(Activity, respond);
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
								dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

								dialog.Show();
							}
							else
							{
								SubscribeChannelButton.Tag = "Subscribe";
								SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribe);
								//Color
								SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
								//icon
								Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribeButton);
								icon.Bounds = new Rect(10, 10, 10, 7);
								SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subscribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(IdChannel);

								//Send API Request here for UnSubscribed
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(UserData.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
						else
						{
							if (SubscribeChannelButton.Tag?.ToString() == "Subscribe")
							{
								SubscribeChannelButton.Tag = "Subscribed";
								SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribed);

								//Color
								SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
								//icon
								Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribedButton);
								icon.Bounds = new Rect(10, 10, 10, 7);
								SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Add The Video to  Subcribed Videos Database
								Events_Insert_SubscriptionsChannel();

								//Send API Request here for Subcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(UserData.Id) });

								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
							}
							else
							{
								SubscribeChannelButton.Tag = "Subscribe";
								SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribe);
								//Color
								SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
								//icon
								Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribeButton);
								icon.Bounds = new Rect(10, 10, 10, 7);
								SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(IdChannel);

								//Send API Request here for UnSubcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(UserData.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
						dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Subcribed), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Tab

		private void SetUpViewPager(ViewPager2 viewPager)
		{
			try
			{
				PlayListFragment = new ChPlayListFragment();
				VideosFragment = new ChVideosFragment();
				ShortsFragment = new ChShortsFragment();
				AboutFragment = new ChAboutFragment();
				ActivitiesFragment = new ChActivitiesFragment();

				Bundle bundle = new Bundle();
				bundle.PutString("ChannelId", IdChannel);

				PlayListFragment.Arguments = bundle;
				VideosFragment.Arguments = bundle;
				ShortsFragment.Arguments = bundle;
				AboutFragment.Arguments = bundle;
				ActivitiesFragment.Arguments = bundle;

				Adapter = new MainTabAdapter(this);
				Adapter.AddFragment(VideosFragment, GetText(Resource.String.Lbl_Videos));
				Adapter.AddFragment(ShortsFragment, GetText(Resource.String.Lbl_Shorts));

				if (AppSettings.AllowPlayLists)
					Adapter.AddFragment(PlayListFragment, GetText(Resource.String.Lbl_PlayLists));

				if (AppSettings.ShowActivities)
					Adapter.AddFragment(ActivitiesFragment, GetText(Resource.String.Lbl_Activities));

				Adapter.AddFragment(AboutFragment, GetText(Resource.String.Lbl_AboutChannal));

				viewPager.Orientation = ViewPager2.OrientationHorizontal;
				viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
				viewPager.Adapter = Adapter;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnConfigureTab(TabLayout.Tab tab, int position)
		{
			try
			{
				tab.SetText(Adapter.GetFragment(position));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
		{
			private readonly UserChannelFragment Fragment;

			public MyOnPageChangeCallback(UserChannelFragment fragment)
			{
				try
				{
					Fragment = fragment;
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}

			public override void OnPageSelected(int position)
			{
				try
				{
					base.OnPageSelected(position);
					var p = position;
					switch (p)
					{
						case 0:
							break;
						case 1:
							if (AppSettings.AllowPlayLists)
								Task.Factory.StartNew(() => Fragment?.PlayListFragment?.StartApiService());
							break;
						case 2:
							break;
						case 3:
							break;
					}
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}
		}

		#endregion

		private async void GetDataUser()
		{
			try
			{
				SetDataUser();

				var data = await ApiRequest.GetChannelData(Activity, IdChannel);
				if (data == null) return;
				UserData = data;
				SetDataUser();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SetDataUser()
		{
			try
			{
				if (UserData != null)
				{
					var name = AppTools.GetNameFinal(UserData);
					CollapsingToolbar.Title = name;
					ChannelNameText.Text = name;

					if (string.IsNullOrEmpty(UserData.SubscribeCount))
						UserData.SubscribeCount = "0";

					TxtSubscribeCount.Text = UserData.SubscribeCount;

					GlideImageLoader.LoadImage(Activity, UserData.Avatar, ImageChannel, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
					Glide.With(Activity).Load(UserData.Cover).Apply(new RequestOptions().FitCenter()).Into(ImageCoverChannel);

					//Verified 
					ChannelVerifiedText.Visibility = UserData.Verified == "1" ? ViewStates.Visible : ViewStates.Gone;

					if (!string.IsNullOrEmpty(UserData.SubscriberPrice) && UserData.SubscriberPrice != "0")
					{
						if (UserData.AmISubscribed == "0")
						{
							//This channel is paid, You must pay to subscribe
							SubscribeChannelButton.Tag = "PaidSubscribe";

							//Color
							SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
							//icon
							Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribeButton);
							icon.Bounds = new Rect(10, 10, 10, 7);
							SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

							var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
							var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
							Console.WriteLine(currency);
							SubscribeChannelButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + UserData.SubscriberPrice;

							if (AppSettings.AllowPlayLists)
							{
								PlayListFragment.MRecycler.Visibility = ViewStates.Gone;
								SetEmptyPageSubscribeChannelWithPaid(PlayListFragment.EmptyStateLayout, PlayListFragment?.Inflated);
							}

							VideosFragment.MRecycler.Visibility = ViewStates.Gone;
							SetEmptyPageSubscribeChannelWithPaid(VideosFragment.EmptyStateLayout, VideosFragment?.Inflated);

							ShortsFragment.MRecycler.Visibility = ViewStates.Gone;
							SetEmptyPageSubscribeChannelWithPaid(ShortsFragment.EmptyStateLayout, ShortsFragment?.Inflated);
						}
						else
						{
							SubscribeChannelButton.Tag = "Subscribed";

							SubscribeChannelButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);

							//Color
							SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
							//icon
							Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribedButton);
							icon.Bounds = new Rect(10, 10, 10, 7);
							SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
						}
					}
					else
					{
						SubscribeChannelButton.Tag = UserData.AmISubscribed == "1" ? "Subscribed" : "Subscribe";

						switch (SubscribeChannelButton.Tag?.ToString())
						{
							case "Subscribed":
								{
									SubscribeChannelButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);

									//Color
									SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
									//icon
									Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribedButton);
									icon.Bounds = new Rect(10, 10, 10, 7);
									SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
									break;
								}
							case "Subscribe":
								{
									SubscribeChannelButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe);

									//Color
									SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
									//icon
									Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribeButton);
									icon.Bounds = new Rect(10, 10, 10, 7);
									SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
									break;
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

		private void Events_Insert_SubscriptionsChannel()
		{
			try
			{
				var sqlEntity = new SqLiteDatabase();

				if (UserData != null)
					sqlEntity.Insert_One_SubscriptionChannel(UserData);

				GlobalContext.LibrarySynchronizer.AddToSubscriptions(UserData);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public async void SetSubscribeChannelWithPaid()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					SubscribeChannelButton.Tag = "Subscribed";
					SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribed);

					//Color
					SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
					//icon
					Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribedButton);
					icon.Bounds = new Rect(10, 10, 10, 7);
					SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

					//Add The Video to  Subscribe Videos Database
					Events_Insert_SubscriptionsChannel();

					//Send API Request here for Subscribe
					var (apiStatus, respond) = await RequestsAsync.Global.AddSubscribeToChannelAsync(IdChannel, "paid");
					if (apiStatus == 200)
					{
						if (respond is MessageObject result)
						{
							Console.WriteLine(result.Message);
							Activity?.RunOnUiThread(() =>
							{
								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();

								if (AppSettings.AllowPlayLists)
									PlayListFragment.ShowEmptyPage();

								VideosFragment.ShowEmptyPage();
								ShortsFragment.ShowEmptyPage();
							});
						}
					}
					else Methods.DisplayReportResult(Activity, respond);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetEmptyPageSubscribeChannelWithPaid(ViewStub emptyStateLayout, View inflated)
		{
			if (emptyStateLayout == null) return;
			try
			{
				inflated ??= emptyStateLayout?.Inflate();

				EmptyStateInflater x = new EmptyStateInflater();
				x?.InflateLayout(inflated, EmptyStateInflater.Type.SubscribeChannelWithPaid);

				var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
				var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
				Console.WriteLine(currency);
				x.TitleText.Text = Activity.GetText(Resource.String.Lbl_SubscribeFor) + " " + currencyIcon + UserData.SubscriberPrice + " " + Activity.GetText(Resource.String.Lbl_AndUnlockAllTheVideos);
				SubscribeChannelButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + UserData.SubscriberPrice;

				if (!x.EmptyStateButton.HasOnClickListeners)
				{
					x.EmptyStateButton.Click += null;
					x.EmptyStateButton.Click += SubscribeChannelWithPaidButtonOnClick;
				}

				emptyStateLayout.Visibility = ViewStates.Visible;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SubscribeChannelWithPaidButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				//This channel is paid, You must pay to subscribe
				var dialog = new MaterialAlertDialogBuilder(Context);
				dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
				dialog.SetMessage(Context.GetText(Resource.String.Lbl_ChannelIsPaid));
				dialog.SetPositiveButton(Context.GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
				{
					try
					{
						if (await AppTools.CheckWallet(Convert.ToDouble(UserData.SubscriberPrice)))
						{
							if (Methods.CheckConnectivity())
							{
								var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("subscribe", UserData.Id);
								if (apiStatus == 200)
								{
									if (respond is MessageObject result)
									{
										Console.WriteLine(result.Message);

										Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
										SetSubscribeChannelWithPaid();
									}
								}
								else Methods.DisplayAndHudErrorResult(Activity, respond);
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
				dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

				dialog.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


	}
}
