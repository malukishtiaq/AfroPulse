using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Playlist;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PlayTube.Activities.Models;
using PlayTube.PlayTubeClient.RestCalls;
using IList = System.Collections.IList;
using Methods = PlayTube.Helpers.Utils.Methods;
using AndroidX.AppCompat.View.Menu;

namespace PlayTube.Activities.Playlist.Adapters
{
	public class PlayListsRowAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		public event EventHandler<PlayListsRowAdapterClickEventArgs> ItemClick;
		public event EventHandler<PlayListsRowAdapterClickEventArgs> ItemLongClick;
		private readonly Activity ActivityContext;
		public ObservableCollection<PlayListVideoObject> PlayListsList = new ObservableCollection<PlayListVideoObject>();
		private readonly RequestOptions Options;
        private readonly LibrarySynchronizer LibrarySynchronizer;

        public PlayListsRowAdapter(Activity context)
		{
			try
			{
				HasStableIds = true;
				ActivityContext = context;
				Options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
					.Transform(new CenterCrop(), new RoundedCorners(15))
					.SetPriority(Priority.High)
					.SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).AutoClone()
					.Error(Resource.Drawable.blackdefault)
					.Placeholder(Resource.Drawable.ImagePlacholder));
                LibrarySynchronizer = new LibrarySynchronizer(context);
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
				//Setup your layout here >> Style_PlayListRowView
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PlayListRowView, parent, false);
				var vh = new PlayListsRowAdapterViewHolder(itemView, OnClick, OnLongClick);
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
				if (viewHolder is PlayListsRowAdapterViewHolder holder)
				{
					var item = PlayListsList[position];
					if (item != null)
					{
                        if (item.Count > 0)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Thumbnail, holder.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable, false, Options);
                        }
                        else
                        {
                            GlideImageLoader.LoadImage(ActivityContext, "blackdefault", holder.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable, false, Options);
                        }

                        holder.TxtPlayListName.Text = Methods.FunString.DecodeString(item.Name);
						holder.TxtViewsCount.Text = item.Count == 0 ? ActivityContext.GetText(Resource.String.Lbl_NoVideos) : item.Count + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
						 
                        if (!holder.MenueView.HasOnClickListeners)
                            holder.MenueView.Click += (sender, args) =>
                            {
                                ContextThemeWrapper ctw = new ContextThemeWrapper(ActivityContext, Resource.Style.PopupMenuStyle);
                                PopupMenu popup = new PopupMenu(ctw, holder.MenueView);
                                popup.MenuInflater?.Inflate(Resource.Menu.Playlist_menu, popup.Menu);
                                popup.Show();
                                popup.MenuItemClick += (o, eventArgs) =>
                                {
                                    try
                                    {
                                        var data = GetItem(holder.BindingAdapterPosition);
                                        var id = eventArgs.Item.ItemId;
                                        switch (id)
                                        {
                                            case Resource.Id.menu_EditPlaylist:
                                                LibrarySynchronizer.EditPlaylist(data);
                                                break;

                                            case Resource.Id.menu_Remove:
                                                OnMenuRemove_Click(data);
                                                break;
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                        ;
                                    }
                                };
                            };
                    }
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

        private void OnMenuRemove_Click(PlayListVideoObject video)
        {
            try
            {
                var index = PlayListsList.IndexOf(PlayListsList.FirstOrDefault(a => a.Id == video.Id));
                if (index != -1)
                {
                    PlayListsList.Remove(video);
                    NotifyDataSetChanged();

                    var data = ListUtils.PlayListVideoObjectList.FirstOrDefault(a => a.Id == video.Id);
                    if (data != null)
                        ListUtils.PlayListVideoObjectList.Remove(data);

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Playlist.DeletePlaylistAsync(video.ListId) });

                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Done), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override int ItemCount => PlayListsList?.Count ?? 0;

		public PlayListVideoObject GetItem(int position)
		{
			return PlayListsList[position];
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

		void OnClick(PlayListsRowAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(PlayListsRowAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is PlayListsRowAdapterViewHolder viewHolder)
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
				var item = PlayListsList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				if (item.Thumbnail != "")
				{
					d.Add(item.Thumbnail);
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

	public class PlayListsRowAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; set; }
		public ImageView VideoImage { get; private set; }
		public TextView TxtPlayListName { get; private set; }
		public TextView TxtViewsCount { get; private set; }
		public ImageView MenueView { get; private set; }

		#endregion

		public PlayListsRowAdapterViewHolder(View itemView, Action<PlayListsRowAdapterClickEventArgs> clickListener, Action<PlayListsRowAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				VideoImage = MainView.FindViewById<ImageView>(Resource.Id.PlayListIcon);
				TxtPlayListName = MainView.FindViewById<TextView>(Resource.Id.PlayListName);
				TxtViewsCount = MainView.FindViewById<TextView>(Resource.Id.PlayListCount);
                MenueView = MainView.FindViewById<ImageView>(Resource.Id.videoMenu);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new PlayListsRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new PlayListsRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class PlayListsRowAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}