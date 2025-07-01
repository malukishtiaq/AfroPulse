using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using PlayTube.Activities.Shorts;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using System;
using System.Collections.Generic;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace PlayTube.Helpers.Utils
{
	public class CustomNavigationController : Java.Lang.Object, View.IOnClickListener
	{
		private readonly Activity MainContext;

		public int PageNumber;

		public readonly List<Fragment> FragmentListTab0 = new List<Fragment>();
		public readonly List<Fragment> FragmentListTab1 = new List<Fragment>();
		public readonly List<Fragment> FragmentListTab2 = new List<Fragment>();
		public readonly List<Fragment> FragmentListTab3 = new List<Fragment>();

		private readonly TabbedMainActivity Context;

		private LinearLayout MainLayout;
		private LinearLayout HomeButton, TrendButton, AddButton, LibraryButton, ShortsButton;
		private ImageView HomeImage, LibraryImage, TrendImage, AddImage, ShortsImage;
		private TextView HomeText, LibraryText, TrendText, AddText, ShortsText;

		public CustomNavigationController(Activity activity)
		{
			try
			{
				MainContext = activity;

				if (activity is TabbedMainActivity cont)
					Context = cont;

				Initialize();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void Initialize()
		{
			try
			{
				MainLayout = MainContext.FindViewById<LinearLayout>(Resource.Id.llMain);
				HomeButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llHome);
				TrendButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llTrend);
				AddButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llAdd);
				ShortsButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llShorts);
				LibraryButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llLibrary);

				if (!AppSettings.ShowShorts)
				{
					ShortsButton.Visibility = ViewStates.Gone;
					MainLayout.WeightSum = 4;
				}

				if (!AppSettings.ShowButtonImport && !AppSettings.ShowButtonUpload && !AppSettings.ShowGoLive && !AppSettings.ShowShorts)
				{
					AddButton.Visibility = ViewStates.Gone;
					MainLayout.WeightSum = 3;
				}

				HomeImage = MainContext.FindViewById<ImageView>(Resource.Id.ivHome);
				LibraryImage = MainContext.FindViewById<ImageView>(Resource.Id.ivLibrary);
				TrendImage = MainContext.FindViewById<ImageView>(Resource.Id.ivTrend);
				AddImage = MainContext.FindViewById<ImageView>(Resource.Id.ivAdd);
				ShortsImage = MainContext.FindViewById<ImageView>(Resource.Id.ivShorts);

				HomeText = MainContext.FindViewById<TextView>(Resource.Id.txtHome);
				LibraryText = MainContext.FindViewById<TextView>(Resource.Id.txtLibrary);
				TrendText = MainContext.FindViewById<TextView>(Resource.Id.txtTrending);
				AddText = MainContext.FindViewById<TextView>(Resource.Id.txtAdd);
				ShortsText = MainContext.FindViewById<TextView>(Resource.Id.txtShorts);

				HomeButton.SetOnClickListener(this);
				TrendButton.SetOnClickListener(this);
				AddButton.SetOnClickListener(this);

				if (AppSettings.ShowShorts)
					ShortsButton.SetOnClickListener(this);

				LibraryButton.SetOnClickListener(this);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private static int OpenNewsFeedTab = 1;
		public void OnClick(View v)
		{
			try
			{
				switch (v.Id)
				{
					case Resource.Id.llHome:
						if (OpenNewsFeedTab == 2)
						{
							OpenNewsFeedTab = 1;
							Context.HomeFragment.LatestHomeTab.MRecycler.ScrollToPosition(0);
						}
						else
						{
							EnableNavigationButton(HomeImage, HomeText);
							OpenNewsFeedTab++;
							PageNumber = 0;
							ShowFragment0();
							AdsGoogle.Ad_AppOpenManager(MainContext);
						}
						break;
					case Resource.Id.llTrend:
						EnableNavigationButton(TrendImage, TrendText);
						OpenNewsFeedTab = 1;
						PageNumber = 1;
						ShowFragment1();
						AdsGoogle.Ad_Interstitial(MainContext);
						break;
					case Resource.Id.llAdd:
						if (!UserDetails.IsLogin)
						{
							PopupDialogController dialog = new PopupDialogController(Context, null, "Login");
							dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Message_Sorry_signin), Context.GetText(Resource.String.Lbl_Ok), Context.GetText(Resource.String.Lbl_Cancel));
							return;
						}

						Context?.CenterButtonOnClick();
						break;
					case Resource.Id.llShorts:

						if (!UserDetails.IsLogin)
						{
							PopupDialogController dialog = new PopupDialogController(Context, null, "Login");
							dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Message_Sorry_signin), Context.GetText(Resource.String.Lbl_Ok), Context.GetText(Resource.String.Lbl_Cancel));
							return;
						}

						Context?.VideoDataWithEventsLoader?.GlobalVideosRelease("All");
						OpenNewsFeedTab = 1;
						PageNumber = 2;

						var intent = new Intent(MainContext, typeof(ShortsVideoDetailsActivity));
						intent.PutExtra("Type", "VideoShorts");
						//intent.PutExtra("DataItem", JsonConvert.SerializeObject(ListUtils.VideoShortsList.ToList()));
						MainContext.StartActivity(intent);

						//EnableNavigationButton(ShortsImage); 
						//ShowFragment2();
						//AdsGoogle.Ad_RewardedVideo(MainContext);
						break;
					case Resource.Id.llLibrary:

						if (!UserDetails.IsLogin)
						{
							PopupDialogController dialog = new PopupDialogController(Context, null, "Login");
							dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Message_Sorry_signin), Context.GetText(Resource.String.Lbl_Ok), Context.GetText(Resource.String.Lbl_Cancel));
							return;
						}

						EnableNavigationButton(LibraryImage, LibraryText);
						OpenNewsFeedTab = 1;
						PageNumber = 3;
						ShowFragment3();
						AdsGoogle.Ad_RewardedInterstitial(MainContext);

						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void ShowHomePage()
		{
			try
			{
				if (OpenNewsFeedTab == 2)
				{
					OpenNewsFeedTab = 1;
					Context.HomeFragment.LatestHomeTab.MRecycler.ScrollToPosition(0);
				}
				else
				{
					EnableNavigationButton(HomeImage, HomeText);
					OpenNewsFeedTab++;
					PageNumber = 0;
					ShowFragment0();
					AdsGoogle.Ad_AppOpenManager(MainContext);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void EnableNavigationButton(ImageView image, TextView text)
		{
			try
			{
				DisableAllNavigationButton();
				//image.Background = MainContext.GetDrawable(Resource.Drawable.shape_bg_bottom_navigation);

				if (text.Text == MainContext.GetString(Resource.String.Lbl_Home))
				{
					image.SetImageResource(Resource.Drawable.pif_bottom_nav_home_icon);
				}

				if (text.Text == MainContext.GetString(Resource.String.Lbl_Trending))
				{
					image.SetImageResource(Resource.Drawable.pif_trending_home_icon);
				}

				if (text.Text == MainContext.GetString(Resource.String.Lbl_Library))
				{
					image.SetImageResource(Resource.Drawable.pif_nav_profile_icon_fill);
				}

				image.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
				text.SetTextColor(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#00020F"));
				text.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
				text.SetTextSize(ComplexUnitType.Sp, 14);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void DisableAllNavigationButton()
		{
			try
			{
				//HomeImage.Background = null;
				HomeImage.SetImageResource(Resource.Drawable.pif_bottom_nav_home_outline);
				HomeImage.SetColorFilter(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B3"));
				HomeText.SetTextColor(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B2"));
				HomeText.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
				HomeText.SetTextSize(ComplexUnitType.Sp, 13);


				//TrendImage.Background = null;
				TrendImage.SetColorFilter(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B3"));
				TrendImage.SetImageResource(Resource.Drawable.pif_trending_icon);
				TrendText.SetTextColor(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B2"));
				TrendText.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
				TrendText.SetTextSize(ComplexUnitType.Sp, 13);

				//ShortsImage.Background = null;
				ShortsImage.SetColorFilter(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B3"));
				ShortsText.SetTextColor(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B2"));
				ShortsText.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
				ShortsText.SetTextSize(ComplexUnitType.Sp, 13);

				//LibraryImage.Background = null;
				LibraryImage.SetColorFilter(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B3"));
				LibraryImage.SetImageResource(Resource.Drawable.icon_username_vector);
				LibraryText.SetTextColor(AppTools.IsTabDark() ? Color.White : Color.ParseColor("#A1A6B2"));
				LibraryText.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
				LibraryText.SetTextSize(ComplexUnitType.Sp, 13);

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public Fragment GetSelectedTabBackStackFragment()
		{
			switch (PageNumber)
			{
				case 0:
					{
						var currentFragment = FragmentListTab0[FragmentListTab0.Count - 2];
						if (currentFragment != null)
							return currentFragment;
						break;
					}
				case 1:
					{
						var currentFragment = FragmentListTab1[FragmentListTab1.Count - 2];
						if (currentFragment != null)
							return currentFragment;
						break;
					}
				case 2:
					{
						var currentFragment = FragmentListTab2[FragmentListTab2.Count - 2];
						if (currentFragment != null)
							return currentFragment;
						break;
					}
				case 3:
					{
						var currentFragment = FragmentListTab3[FragmentListTab3.Count - 2];
						if (currentFragment != null)
							return currentFragment;
						break;
					}
				default:
					return null;

			}

			return null;
		}

		public int GetCountFragment()
		{
			try
			{
				switch (PageNumber)
				{
					case 0:
						return FragmentListTab0.Count > 1 ? FragmentListTab0.Count : 0;
					case 1:
						return FragmentListTab1.Count > 1 ? FragmentListTab1.Count : 0;
					case 2:
						return FragmentListTab2.Count > 1 ? FragmentListTab2.Count : 0;
					case 3:
						return FragmentListTab3.Count > 1 ? FragmentListTab3.Count : 0;
					default:
						return 0;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return 0;
			}
		}

		public static void HideFragmentFromList(List<Fragment> fragmentList, FragmentTransaction ft)
		{
			try
			{
				if (fragmentList.Count < 0)
					return;

				foreach (var fra in fragmentList)
				{
					if (fra.IsVisible)
						ft.Hide(fra);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void DisplayFragment(Fragment newFragment)
		{
			try
			{
				FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

				HideFragmentFromList(FragmentListTab0, ft);
				HideFragmentFromList(FragmentListTab1, ft);
				HideFragmentFromList(FragmentListTab2, ft);
				HideFragmentFromList(FragmentListTab3, ft);

				switch (PageNumber)
				{
					case 0:
						{
							if (!FragmentListTab0.Contains(newFragment))
								FragmentListTab0.Add(newFragment);
							break;
						}
					case 1:
						{
							if (!FragmentListTab1.Contains(newFragment))
								FragmentListTab1.Add(newFragment);
							break;
						}
					case 2:
						{
							if (!FragmentListTab2.Contains(newFragment))
								FragmentListTab2.Add(newFragment);
							break;
						}
					case 3:
						{
							if (!FragmentListTab3.Contains(newFragment))
								FragmentListTab3.Add(newFragment);
							break;
						}
				}

				if (!newFragment.IsAdded)
					ft.Add(Resource.Id.mainFragmentHolder, newFragment, newFragment.Id.ToString());

				ft.Show(newFragment)?.Commit();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void RemoveFragment(Fragment oldFragment)
		{
			try
			{
				FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

				switch (PageNumber)
				{
					case 0:
						{
							if (FragmentListTab0.Contains(oldFragment))
								FragmentListTab0.Remove(oldFragment);
							break;
						}
					case 1:
						{
							if (FragmentListTab1.Contains(oldFragment))
								FragmentListTab1.Remove(oldFragment);
							break;
						}
					case 2:
						{
							if (FragmentListTab2.Contains(oldFragment))
								FragmentListTab2.Remove(oldFragment);
							break;
						}
					case 3:
						{
							if (FragmentListTab3.Contains(oldFragment))
								FragmentListTab3.Remove(oldFragment);
							break;
						}
				}


				HideFragmentFromList(FragmentListTab0, ft);
				HideFragmentFromList(FragmentListTab1, ft);
				HideFragmentFromList(FragmentListTab2, ft);
				HideFragmentFromList(FragmentListTab3, ft);

				if (oldFragment.IsAdded)
					ft.Remove(oldFragment);

				switch (PageNumber)
				{
					case 0:
						{
							var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
							ft.Show(currentFragment)?.Commit();
							break;
						}
					case 1:
						{
							var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
							ft.Show(currentFragment)?.Commit();
							break;
						}
					case 2:
						{
							var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
							ft.Show(currentFragment)?.Commit();
							break;
						}
					case 3:
						{
							var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
							ft.Show(currentFragment)?.Commit();
							break;
						}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnBackStackClickFragment()
		{
			try
			{
				switch (PageNumber)
				{
					case 0 when FragmentListTab0.Count > 1:
						{
							var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
							if (currentFragment != null)
								RemoveFragment(currentFragment);
							break;
						}
					case 0:
						Context.Finish();
						break;
					case 1 when FragmentListTab1.Count > 1:
						{
							var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
							if (currentFragment != null)
								RemoveFragment(currentFragment);
							break;
						}
					case 1:
						Context.Finish();
						break;
					case 2 when FragmentListTab2.Count > 1:
						{
							var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
							if (currentFragment != null)
								RemoveFragment(currentFragment);
							break;
						}
					case 2:
						Context.Finish();
						break;
					case 3 when FragmentListTab3.Count > 1:
						{
							var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
							if (currentFragment != null)
								RemoveFragment(currentFragment);
							break;
						}
					case 3:
						Context.Finish();
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void ShowFragment0()
		{
			try
			{
				if (FragmentListTab0.Count <= 0)
					return;
				var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
				if (currentFragment != null)
					DisplayFragment(currentFragment);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void ShowFragment1()
		{
			try
			{
				if (FragmentListTab1.Count <= 0) return;
				var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
				if (currentFragment != null)
					DisplayFragment(currentFragment);

				Context.InAppReview();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void ShowFragment2()
		{
			try
			{
				if (FragmentListTab2.Count <= 0) return;
				var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
				if (currentFragment != null)
					DisplayFragment(currentFragment);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void ShowFragment3()
		{
			try
			{
				if (FragmentListTab3.Count <= 0) return;
				var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
				if (currentFragment != null)
					DisplayFragment(currentFragment);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static bool BringFragmentToTop(Fragment tobeshown, FragmentManager fragmentManager, List<Fragment> videoFrameLayoutFragments)
		{
			try
			{
				if (tobeshown != null)
				{
					FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();


					foreach (var f in fragmentManager.Fragments)
					{
						if (videoFrameLayoutFragments.Contains(f))
						{
							if (f == tobeshown)
								fragmentTransaction.Show(f);
							else
								fragmentTransaction.Hide(f);
						}

					}

					fragmentTransaction?.Commit();

					return true;
				}
				else
				{
					FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();

					foreach (var f in videoFrameLayoutFragments)
					{
						fragmentTransaction.Hide(f);
					}

					fragmentTransaction?.Commit();
				}
				return false;

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return false;
			}
		}

	}
}