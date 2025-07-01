using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.BottomSheet;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PlayTube.Activities.ChannelPopular
{
	public class ChannelPopularFilterBottomDialogFragment : BottomSheetDialogFragment
	{
		#region Variables Basic

		private TextView BtnReset;
		private AppCompatButton ViewsButton, SubscribersButton, MostActiveButton, TodayButton, WeekButton, MonthButton, YearButton, AllTimeButton;
		private AppCompatButton ButtonApply;
		private string TimeChecked = "all_time", SortByChecked = "views";

		#endregion

		#region General

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// create ContextThemeWrapper from the original Activity Context with the custom theme
				Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);

				// clone the inflater using the ContextThemeWrapper
				LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
				View view = localInflater?.Inflate(Resource.Layout.ButtomSheetChannelPopularFilter, container, false);
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
				//Get Value And Set Toolbar
				InitComponent(view);
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
				ViewsButton = view.FindViewById<AppCompatButton>(Resource.Id.ViewsButton);
				SubscribersButton = view.FindViewById<AppCompatButton>(Resource.Id.SubscribersButton);
				MostActiveButton = view.FindViewById<AppCompatButton>(Resource.Id.MostActiveButton);

				TodayButton = view.FindViewById<AppCompatButton>(Resource.Id.TodayButton);
				WeekButton = view.FindViewById<AppCompatButton>(Resource.Id.WeekButton);
				MonthButton = view.FindViewById<AppCompatButton>(Resource.Id.MonthButton);
				YearButton = view.FindViewById<AppCompatButton>(Resource.Id.YearButton);
				AllTimeButton = view.FindViewById<AppCompatButton>(Resource.Id.AllTimeButton);

				BtnReset = view.FindViewById<TextView>(Resource.Id.Resetbutton);
				ButtonApply = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

				ViewsButton.Click += ViewsButtonOnClick;
				SubscribersButton.Click += SubscribersButtonOnClick;
				MostActiveButton.Click += MostActiveButtonOnClick;
				TodayButton.Click += TodayButtonOnClick;
				WeekButton.Click += WeekButtonOnClick;
				MonthButton.Click += MonthButtonOnClick;
				YearButton.Click += YearButtonOnClick;
				AllTimeButton.Click += AllTimeButtonOnClick;

				ButtonApply.Click += ButtonApplyOnClick;
				BtnReset.Click += BtnResetOnClick;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void AllTimeButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				TimeChecked = "all_time";

				SetBackgroundButton(AllTimeButton, true);

				SetBackgroundButton(TodayButton, false);
				SetBackgroundButton(WeekButton, false);
				SetBackgroundButton(MonthButton, false);
				SetBackgroundButton(YearButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void YearButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				TimeChecked = "this_year";

				SetBackgroundButton(YearButton, true);

				SetBackgroundButton(TodayButton, false);
				SetBackgroundButton(WeekButton, false);
				SetBackgroundButton(MonthButton, false);
				SetBackgroundButton(AllTimeButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MonthButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				TimeChecked = "this_month";

				SetBackgroundButton(MonthButton, true);

				SetBackgroundButton(TodayButton, false);
				SetBackgroundButton(WeekButton, false);
				SetBackgroundButton(YearButton, false);
				SetBackgroundButton(AllTimeButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void WeekButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				TimeChecked = "this_week";

				SetBackgroundButton(WeekButton, true);

				SetBackgroundButton(TodayButton, false);
				SetBackgroundButton(MonthButton, false);
				SetBackgroundButton(YearButton, false);
				SetBackgroundButton(AllTimeButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TodayButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				TimeChecked = "today";

				SetBackgroundButton(TodayButton, true);

				SetBackgroundButton(WeekButton, false);
				SetBackgroundButton(MonthButton, false);
				SetBackgroundButton(YearButton, false);
				SetBackgroundButton(AllTimeButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void MostActiveButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				SortByChecked = "most_active";

				SetBackgroundButton(MostActiveButton, true);

				SetBackgroundButton(ViewsButton, false);
				SetBackgroundButton(SubscribersButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SubscribersButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				SortByChecked = "subscribers";

				SetBackgroundButton(SubscribersButton, true);

				SetBackgroundButton(ViewsButton, false);
				SetBackgroundButton(MostActiveButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ViewsButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				SortByChecked = "views";

				SetBackgroundButton(ViewsButton, true);

				SetBackgroundButton(SubscribersButton, false);
				SetBackgroundButton(MostActiveButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Save and sent data and set new search 
		private void ButtonApplyOnClick(object sender, EventArgs e)
		{
			try
			{
				UserDetails.FilterTypeSortBy = SortByChecked;
				UserDetails.FilterTime = TimeChecked;

				var channelPopularFragment = TabbedMainActivity.GetInstance().TrendingFragment.AllChannelPopularFragment;
				if (channelPopularFragment != null)
				{
					if (channelPopularFragment.MAdapter != null)
					{
						channelPopularFragment.SwipeRefreshLayout.Refreshing = true;
						channelPopularFragment.SwipeRefreshLayout.Enabled = true;

						channelPopularFragment.EmptyStateLayout.Visibility = ViewStates.Gone;
						channelPopularFragment.MRecycler.Visibility = ViewStates.Visible;

						channelPopularFragment.MAdapter.ChannelList.Clear();
						channelPopularFragment.MAdapter.NotifyDataSetChanged();
					}

					Task.Factory.StartNew(() => channelPopularFragment.StartApiService());
				}

				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Reset data 
		private void BtnResetOnClick(object sender, EventArgs e)
		{
			try
			{
				TimeChecked = "all_time";

				SetBackgroundButton(AllTimeButton, true);

				SetBackgroundButton(TodayButton, false);
				SetBackgroundButton(WeekButton, false);
				SetBackgroundButton(MonthButton, false);
				SetBackgroundButton(YearButton, false);

				//===========

				SortByChecked = "views";
				SetBackgroundButton(ViewsButton, true);

				SetBackgroundButton(SubscribersButton, false);
				SetBackgroundButton(MostActiveButton, false);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		private void SetBackgroundButton(AppCompatButton button, bool check)
		{
			try
			{
				if (check)
				{
					button.SetBackgroundResource(Resource.Drawable.round_button_pressed);
					button.SetTextColor(Color.White);
				}
				else
				{
					button.SetBackgroundResource(Resource.Drawable.round_button_normal);
					button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}
}