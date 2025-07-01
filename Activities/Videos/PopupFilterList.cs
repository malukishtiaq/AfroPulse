using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using PlayTube.Activities.Playlist.Adapters;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Playlist;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlayTube.Activities.Videos
{
	public class PopupFilterList
	{
		private readonly Activity ActivityContext;
		private readonly PlayListsRowAdapter PlaylistAdapter;

		public LinearLayout TopLayout;
		public TextView TxtSwap;
		private readonly string TypePage = "";

		public PopupFilterList(View view, Activity activity, PlayListsRowAdapter adapter)
		{
			try
			{
				ActivityContext = activity;
				PlaylistAdapter = adapter;

				TypePage = "Playlist";
				NameType = ActivityContext.GetText(Resource.String.Lbl_RecentlyAdded);

				Init(view);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void Init(View view)
		{
			try
			{
				TopLayout = view.FindViewById<LinearLayout>(Resource.Id.Toplayout);
				TxtSwap = view.FindViewById<TextView>(Resource.Id.swaptext);
				TxtSwap.Click += TxtSwapOnClick;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private static string Filter = "0";
		private static string NameType = "";
		private ImageView MenuCheckAscending, MenuCheckDescending, MenuCheckDataAdded;
		private void TxtSwapOnClick(object sender, EventArgs e)
		{
			try
			{
				LayoutInflater layoutInflater = (LayoutInflater)ActivityContext?.GetSystemService(Context.LayoutInflaterService);
				View popupView = layoutInflater?.Inflate(Resource.Layout.PopupFilterListLayout, null);

				int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 180, ActivityContext.Resources.DisplayMetrics);
				var popupWindow = new PopupWindow(popupView, px, ViewGroup.LayoutParams.WrapContent);

				var menuAscending = popupView.FindViewById<LinearLayout>(Resource.Id.menu_ascending);
				MenuCheckAscending = popupView.FindViewById<ImageView>(Resource.Id.menu_check_ascending);

				var menuDescending = popupView.FindViewById<LinearLayout>(Resource.Id.menu_descending);
				MenuCheckDescending = popupView.FindViewById<ImageView>(Resource.Id.menu_check_descending);

				var menuDataAdded = popupView.FindViewById<LinearLayout>(Resource.Id.menu_dataAdded);
				MenuCheckDataAdded = popupView.FindViewById<ImageView>(Resource.Id.menu_check_dataAdded);

				CheckType(Filter);

				//By name Ascending
				menuAscending.Click += (sender, args) =>
				{
					try
					{
						Filter = "1";
						NameType = ActivityContext.GetText(Resource.String.Lbl_Ascending);

						MenuCheckAscending.SetImageResource(Resource.Drawable.ic_check_circle);

						CheckType(Filter);
						popupWindow.Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				//By name Descending
				menuDescending.Click += (sender, args) =>
				{
					try
					{
						Filter = "2";
						NameType = ActivityContext.GetText(Resource.String.Lbl_Descending);

						MenuCheckDescending.SetImageResource(Resource.Drawable.ic_check_circle);

						CheckType(Filter);
						popupWindow.Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				//By Recently Added
				menuDataAdded.Click += (sender, args) =>
				{
					try
					{
						Filter = "3";
						NameType = ActivityContext.GetText(Resource.String.Lbl_RecentlyAdded);

						MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_check_circle);

						CheckType(Filter);
						popupWindow.Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				MenuCheckAscending.SetImageResource(Resource.Drawable.ic_uncheck_circle);

				popupWindow.SetBackgroundDrawable(new ColorDrawable());
				popupWindow.Focusable = true;
				popupWindow.ClippingEnabled = true;
				popupWindow.OutsideTouchable = false;
				popupWindow.DismissEvent += delegate (object sender, EventArgs args)
				{
					try
					{
						popupWindow.Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				popupWindow.ShowAsDropDown(TxtSwap);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void CheckType(string type)
		{
			try
			{
				TxtSwap.Text = NameType;

				if (type == "0")
				{
					TxtSwap.Text = ActivityContext.GetText(Resource.String.Lbl_RecentlyAdded);
					return;
				}

				switch (type)
				{
					case "1":
						MenuCheckAscending.SetImageResource(Resource.Drawable.ic_check_circle);

						MenuCheckDescending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
						MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_uncheck_circle);
						break;
					case "2":
						MenuCheckDescending.SetImageResource(Resource.Drawable.ic_check_circle);

						MenuCheckAscending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
						MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_uncheck_circle);
						break;
					case "3":
						MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_check_circle);

						MenuCheckAscending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
						MenuCheckDescending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
						break;
				}

				if (TypePage == "Playlist")
				{
					List<PlayListVideoObject> sortList = new List<PlayListVideoObject>(PlaylistAdapter.PlayListsList);
					switch (type)
					{
						case "1":
							sortList = PlaylistAdapter.PlayListsList.OrderBy(o => o.Name).ToList();
							break;
						case "2":
							sortList = PlaylistAdapter.PlayListsList.OrderByDescending(o => o.Name).ToList();
							break;
						case "3":
							sortList = PlaylistAdapter.PlayListsList.OrderBy(o => o.Time).ToList();
							break;
					}

					PlaylistAdapter.PlayListsList = new ObservableCollection<PlayListVideoObject>(sortList);
					PlaylistAdapter.NotifyDataSetChanged();

					MainSettings.SharedData?.Edit()?.PutString("popup_filter_playlist_key", type)?.Commit();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}