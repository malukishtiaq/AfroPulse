using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Article.Adapters
{
	public class ArticlesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		private readonly Activity ActivityContext;
		public readonly ArticlesFragment AFragment;
		public readonly Dictionary<int, string> CategoryColor = new Dictionary<int, string>();
		public ObservableCollection<ArticleDataObject> ArticlesList = new ObservableCollection<ArticleDataObject>();

		public ArticlesAdapter(Activity context, ArticlesFragment fragment)
		{
			try
			{
				HasStableIds = true;
				ActivityContext = context;
				AFragment = fragment;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override int ItemCount => ArticlesList?.Count ?? 0;

		public event EventHandler<ArticlesAdapterClickEventArgs> ItemClick;
		public event EventHandler<ArticlesAdapterClickEventArgs> ItemLongClick;

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> Style_ArticleView
				var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ArticleView, parent, false);
				var vh = new ArticlesAdapterViewHolder(itemView, OnClick, OnLongClick);
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

				if (viewHolder is ArticlesAdapterViewHolder holder)
				{
					var item = ArticlesList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, !string.IsNullOrEmpty(item.Image) ? item.Image : "blackdefault", holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

						GlideImageLoader.LoadImage(ActivityContext, !string.IsNullOrEmpty(item.UserData?.Avatar) ? item.UserData.Avatar : "no_profile_image_circle", holder.ImageChannel, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

						var color = Methods.FunString.RandomColor().Item1;
						holder.Category.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(color));
						CategoryColor.Add(item.Id, color);

						string name = Methods.FunString.DecodeString(CategoriesController.ListCategories?.FirstOrDefault(a => a.Id == item.Category)?.Name);
						if (string.IsNullOrEmpty(name))
							name = ActivityContext.GetString(Resource.String.Lbl_Unknown);

						holder.Category.Text = name;

						holder.Title.Text = Methods.FunString.DecodeString(item.Title);
						holder.ChannelName.Text = AppTools.GetNameFinal(item.UserData);
						holder.ChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.UserData?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

						holder.ViewsCount.Text = " | " + item.Views + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

						if (!holder.InfoContainer.HasOnClickListeners)
							holder.InfoContainer.Click += (sender, args) =>
							{
								try
								{
									AFragment.OpenChannel(item);
								}
								catch (Exception e)
								{
									Methods.DisplayReportResultTrack(e);
								}
							};
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public ArticleDataObject GetItem(int position)
		{
			return ArticlesList[position];
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

		private void OnClick(ArticlesAdapterClickEventArgs args)
		{
			ItemClick?.Invoke(this, args);
		}

		private void OnLongClick(ArticlesAdapterClickEventArgs args)
		{
			ItemLongClick?.Invoke(this, args);
		}

		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is ArticlesAdapterViewHolder viewHolder)
				{
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.Image);
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.ImageChannel);
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
				var item = ArticlesList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				var image = !string.IsNullOrEmpty(item.Image) ? item.Image : "";
				var imageAvatar = !string.IsNullOrEmpty(item.UserData?.Avatar) ? item.UserData.Avatar : "";

				if (!string.IsNullOrEmpty(image))
				{
					d.Add(image);
					d.Add(imageAvatar);

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
			return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
		}
	}

	public class ArticlesAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; }

		public ImageView Image { get; private set; }
		public TextView Title { get; private set; }
		public TextView Category { get; private set; }
		public LinearLayout InfoContainer { get; private set; }
		public ImageView ImageChannel { get; private set; }
		public TextView ChannelName { get; private set; }
		public TextView ViewsCount { get; private set; }

		#endregion
		public ArticlesAdapterViewHolder(View itemView, Action<ArticlesAdapterClickEventArgs> clickListener, Action<ArticlesAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
				Category = MainView.FindViewById<TextView>(Resource.Id.textCategory);
				Title = MainView.FindViewById<TextView>(Resource.Id.Title);

				InfoContainer = MainView.FindViewById<LinearLayout>(Resource.Id.info_container);

				ImageChannel = MainView.FindViewById<ImageView>(Resource.Id.Image_Channel);
				ChannelName = MainView.FindViewById<TextView>(Resource.Id.ChannelName);
				ViewsCount = MainView.FindViewById<TextView>(Resource.Id.Views_Count);

				//Event
				itemView.Click += (sender, e) => clickListener(new ArticlesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new ArticlesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

	}

	public class ArticlesAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}