using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Activities.Models;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Videos.Adapters
{
	public class WatchLaterVideoRowAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, IVideoMenuListener
	{
		public event EventHandler<WatchLaterVideoAdapterClickEventArgs> ItemClick;
		public event EventHandler<WatchLaterVideoAdapterClickEventArgs> ItemLongClick;
		private readonly Activity ActivityContext;
		public ObservableCollection<DataWatchLaterVideos> VideoList = new ObservableCollection<DataWatchLaterVideos>();
		private readonly LibrarySynchronizer LibrarySynchronizer;
		private readonly TabbedMainActivity GlobalContext;
		private readonly RequestOptions Options;

		public WatchLaterVideoRowAdapter(Activity context)
		{
			try
			{
				HasStableIds = true;
				ActivityContext = context;
				LibrarySynchronizer = new LibrarySynchronizer(context);
				GlobalContext = TabbedMainActivity.GetInstance();

				Options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
					.Transform(new CenterCrop(), new RoundedCorners(25))
					.SetPriority(Priority.High)
					.SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).AutoClone()
					.Error(Resource.Drawable.ImagePlacholder)
					.Placeholder(Resource.Drawable.ImagePlacholder));
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> Video_Row_View
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_VideoRowView, parent, false);
				var vh = new WatchLaterVideoRowAdapterViewHolder(itemView, OnClick, OnLongClick);

				return vh;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return null;
			}
		}

		// Replace the contents of a view (invoked by the layout manager)
		public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
		{
			try
			{
				if (viewHolder is WatchLaterVideoRowAdapterViewHolder holder)
				{
					var item = VideoList[position];
					if (item.Videos?.VideoAdClass != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, item.Videos?.VideoAdClass.Thumbnail, holder.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable, false, Options);

						holder.TxtDuration.Text = Methods.Time.SplitStringDuration(item.Videos?.VideoAdClass.Duration);
						holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Videos?.VideoAdClass.Title);

						holder.TxtChannelName.Text = AppTools.GetNameFinal(item.Videos?.VideoAdClass.Owner?.OwnerClass);
						holder.TxtChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Videos?.VideoAdClass.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

						holder.TxtViewsCount.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.Videos?.VideoAdClass.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

						if (!holder.MenuView.HasOnClickListeners)
						{
							holder.MenuView.Click += (sender, args) =>
							{
								var data = GetItem(holder.BindingAdapterPosition);
								VideoMenuBottomSheets videoMenuBottomSheets = new VideoMenuBottomSheets(data.Videos?.VideoAdClass, this, "WatchLater");
								videoMenuBottomSheets.Show(GlobalContext.SupportFragmentManager, videoMenuBottomSheets.Tag);
							};

							holder.TxtChannelName.Click += (sender, args) =>
							{
								var data = GetItem(holder.BindingAdapterPosition);
								GlobalContext?.ShowUserChannelFragment(data.Videos?.VideoAdClass.Owner?.OwnerClass, data.Videos?.VideoAdClass.Owner?.OwnerClass.Id);
							};
						}

						//Set Badge on videos
						AppTools.ShowGlobalBadgeSystem(holder.VideoType, item.Videos?.VideoAdClass);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void RemoveVideo(VideoDataObject data)
		{
			try
			{
				var check = VideoList.FirstOrDefault(a => a.Id == data.Id);
				if (check != null)
				{
					var index = VideoList.IndexOf(check);
					if (index != -1)
					{
						VideoList.Remove(check);
						NotifyItemRemoved(index);
						NotifyItemRangeChanged(index, ItemCount);

						Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Removed), ToastLength.Short)?.Show();

						var dataObject = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.Id == data.Id);
						if (dataObject == null)
						{
							ListUtils.GlobalNotInterestedList.Add(data);
						}

					}
					if (Methods.CheckConnectivity())
						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddDeleteNotInterestedAsync(data.Id, true) });
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override int ItemCount => VideoList?.Count ?? 0;

		public DataWatchLaterVideos GetItem(int position)
		{
			return VideoList[position];
		}

		public override long GetItemId(int position)
		{
			try
			{
				return position;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return 0;
			}
		}

		public override int GetItemViewType(int position)
		{
			try
			{
				return position;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return 0;
			}
		}
		void OnClick(WatchLaterVideoAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(WatchLaterVideoAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is WatchLaterVideoRowAdapterViewHolder viewHolder)
				{
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.VideoImage);
				}
				base.OnViewRecycled(holder);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
		public IList GetPreloadItems(int p0)
		{
			try
			{
				var d = new List<string>();
				var item = VideoList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				if (item.Videos?.VideoAdClass?.Thumbnail != "")
				{
					d.Add(item.Videos?.VideoAdClass.Thumbnail);
					return d;
				}

				return d;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return Collections.SingletonList(p0);
			}
		}

		public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
		{
			return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
				.Apply(new RequestOptions().CenterCrop());
		}
	}

	public class WatchLaterVideoRowAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; set; }
		public ImageView VideoImage { get; private set; }
		public TextView TxtDuration { get; private set; }
		public TextView TxtTitle { get; private set; }
		public TextView TxtChannelName { get; private set; }
		public TextView TxtViewsCount { get; private set; }
		public TextView VideoType { get; private set; }
		public ImageView MenuView { get; private set; }

		#endregion

		public WatchLaterVideoRowAdapterViewHolder(View itemView, Action<WatchLaterVideoAdapterClickEventArgs> clickListener, Action<WatchLaterVideoAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				VideoImage = MainView.FindViewById<ImageView>(Resource.Id.Imagevideo);
				VideoType = MainView.FindViewById<TextView>(Resource.Id.videoType);
				TxtDuration = MainView.FindViewById<TextView>(Resource.Id.duration);

				TxtTitle = MainView.FindViewById<TextView>(Resource.Id.Title);
				MenuView = MainView.FindViewById<ImageView>(Resource.Id.videoMenu);

				TxtChannelName = MainView.FindViewById<TextView>(Resource.Id.ChannelName);
				TxtViewsCount = MainView.FindViewById<TextView>(Resource.Id.Views_Count);

				//Create an Event
				itemView.Click += (sender, e) => clickListener(new WatchLaterVideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new WatchLaterVideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class WatchLaterVideoAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}