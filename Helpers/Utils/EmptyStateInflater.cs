using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using System;

namespace PlayTube.Helpers.Utils
{
	public class EmptyStateInflater
	{
		public AppCompatButton EmptyStateButton;
		public ImageView EmptyStateIcon;
		public TextView DescriptionText;
		public TextView TitleText;

		public enum Type
		{
			NoConnection,
			NoSearchResult,
			SomThingWentWrong,
			NoComments,
			NoReplies,
			NoNotification,
			NoMessage,
			NoVideo,
			NoVideoByCat,
			NoArticle,
			NoRecentlyWatched,
			NoLiked,
			NoPlayLists,
			NoTrending,
			NoWatchLater,
			NoWatchOfflineVideos,
			NoChannels,
			NoPaid,
			NoBlock,
			SubscribeChannelWithPaid,
			Login,
			NoSessions,
			NoActivities,
			NoStock,
			NoMovies,
		}

		public void InflateLayout(View inflated, Type type)
		{
			try
			{
				EmptyStateIcon = (ImageView)inflated.FindViewById(Resource.Id.emtyicon);
				TitleText = (TextView)inflated.FindViewById(Resource.Id.headText);
				DescriptionText = (TextView)inflated.FindViewById(Resource.Id.seconderyText);
				EmptyStateButton = (AppCompatButton)inflated.FindViewById(Resource.Id.button);

				switch (type)
				{
					case Type.NoConnection:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.IosThunderstormOutline);
						EmptyStateIcon.SetImageResource(Resource.Drawable.img_no_internet);
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_TitleText);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_DescriptionText);
						EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_Button);
						break;
					case Type.NoSearchResult:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Search);
						EmptyStateIcon.SetImageResource(Resource.Drawable.pif_search_not_found);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#00000000"));
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_search);
						DescriptionText.Text = "";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.SomThingWentWrong:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Close);
						EmptyStateIcon.SetImageResource(Resource.Drawable.ic_warning_po);
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_TitleText);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_DescriptionText);
						EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_Button);
						break;
					case Type.NoComments:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatbubble);
						//EmptyStateIcon.SetImageResource(Resource.Drawable.icon_comment_vector);
						//TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoComments_TitleText);
						//DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoComments_DescriptionText);

						EmptyStateIcon.SetImageResource(Resource.Drawable.pif_no_chat_history);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#00000000"));
						//var ll = new LinearLayout.LayoutParams(223, 173);
						//EmptyStateIcon.LayoutParameters = new ViewGroup.LayoutParams(223, 173);

						TitleText.Visibility = ViewStates.Gone;
						DescriptionText.Visibility = ViewStates.Gone;
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoReplies:
						EmptyStateIcon.Visibility = ViewStates.Gone;
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoReplies);
						DescriptionText.Text = " ";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoNotification:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Notifications);
						EmptyStateIcon.SetImageResource(Resource.Drawable.pif_notification_not_found);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#CDCDD7"));
						//TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_notifications);
						//DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_notifications);
						TitleText.Visibility = ViewStates.Gone;
						DescriptionText.Visibility = ViewStates.Gone;
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoMessage:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatbox);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_message_vector);
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Lastmessages);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Lastmessages);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoArticle:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, EmptyStateIcon, "\uf15c");
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_blog_vector);
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Article);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Article);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoVideo:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.pif_no_video_available);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#00000000"));
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoVideoAvailable);
						//DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_No_videos_found_for_now);
						DescriptionText.Visibility = ViewStates.Gone;
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoVideoByCat:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.ic_no_video_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Emptyvideos);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Category);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoRecentlyWatched:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_recently_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Dont_have_any_videos);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Historyvideo);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoLiked:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_like_video_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Dont_have_any_videos);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_likedvideo);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoPlayLists:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.pif_no_play_list);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#00000000"));
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_PlayLists);
						//DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_No_videos_found_for_now);
						DescriptionText.Visibility = ViewStates.Gone;
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoTrending:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_fire_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Emptyvideos);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Trending);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoWatchLater:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_time_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Dont_have_any_videos);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_watchlater);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoWatchOfflineVideos:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_download_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Dont_have_any_videos);
						DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_watchoffline);
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoChannels:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_username_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoChannelsFound);
						DescriptionText.Text = "";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoPaid:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_dollars_vector);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#00000000"));
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoPaidVideosFound);
						DescriptionText.Text = "";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoBlock:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
						EmptyStateIcon.SetImageResource(Resource.Drawable.pif_blocked_user);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#00000000"));
						//TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoBlockUsers);
						TitleText.Visibility = ViewStates.Gone;
						DescriptionText.Text = " ";
						DescriptionText.Visibility = ViewStates.Gone;
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.SubscribeChannelWithPaid:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.LogoUsd);
						EmptyStateIcon.SetImageResource(Resource.Drawable.ic_hand_usd_vector);

						TitleText.Text = "";
						DescriptionText.Text = " ";
						EmptyStateButton.Visibility = ViewStates.Visible;
						break;
					case Type.Login:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Bulb);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_username_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Please_sign_in_view_comment);
						DescriptionText.Text = " ";
						EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_Login);
						break;
					case Type.NoSessions:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Fingerprint);
						EmptyStateIcon.SetImageResource(Resource.Drawable.ic_hand_usd_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Sessions);
						DescriptionText.Text = "";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoActivities:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.pif_no_activity);
						EmptyStateIcon.SetColorFilter(Color.ParseColor("#00000000"));
						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Activities);
						DescriptionText.Text = "";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoStock:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.ic_no_video_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Dont_have_any_videos);
						DescriptionText.Text = "";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
					case Type.NoMovies:
						//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
						EmptyStateIcon.SetImageResource(Resource.Drawable.icon_movies_vector);

						TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Movies);
						DescriptionText.Text = "";
						EmptyStateButton.Visibility = ViewStates.Gone;
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}