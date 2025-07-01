using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Movies.Adapters
{
	public class MoviesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		private readonly Activity ActivityContext;
		public ObservableCollection<VideoDataObject> MoviesList = new ObservableCollection<VideoDataObject>();
		private readonly Dictionary<string, string> CountriesArray = new Dictionary<string, string>();
		public MoviesAdapter(Activity context)
		{
			try
			{
				HasStableIds = true;
				ActivityContext = context;
				CountriesArray = AppTools.GetCountryList(ActivityContext);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override int ItemCount => MoviesList?.Count ?? 0;

		public event EventHandler<MoviesAdapterClickEventArgs> ItemClick;
		public event EventHandler<MoviesAdapterClickEventArgs> ItemLongClick;

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> Style_MoviesView
				var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoviesView, parent, false);
				var vh = new MoviesAdapterViewHolder(itemView, OnClick, OnLongClick);
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
				if (viewHolder is MoviesAdapterViewHolder holder)
				{
					var item = MoviesList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, item.Thumbnail, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
						holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Title);

						var check = CountriesArray?.FirstOrDefault(a => a.Key == item.Country).Value;
						if (check != null)
						{
							holder.TxtCountry.Text = check;
							holder.MovieRelease.Text = item.MovieRelease;
						}
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public VideoDataObject GetItem(int position)
		{
			return MoviesList[position];
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

		private void OnClick(MoviesAdapterClickEventArgs args)
		{
			ItemClick?.Invoke(this, args);
		}

		private void OnLongClick(MoviesAdapterClickEventArgs args)
		{
			ItemLongClick?.Invoke(this, args);
		}

		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is MoviesAdapterViewHolder viewHolder)
				{
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.Image);
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
				var item = MoviesList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				if (!string.IsNullOrEmpty(item.Thumbnail))
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
			return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
		}
	}

	public class MoviesAdapterViewHolder : RecyclerView.ViewHolder
	{
		public MoviesAdapterViewHolder(View itemView, Action<MoviesAdapterClickEventArgs> clickListener, Action<MoviesAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				Image = MainView.FindViewById<ImageView>(Resource.Id.image);
				TxtTitle = MainView.FindViewById<TextView>(Resource.Id.name);
				TxtCountry = MainView.FindViewById<TextView>(Resource.Id.country);
				MovieRelease = MainView.FindViewById<TextView>(Resource.Id.MovieRelease);
				//Event
				itemView.Click += (sender, e) => clickListener(new MoviesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new MoviesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#region Variables Basic

		public View MainView { get; }

		public ImageView Image { get; private set; }
		public TextView TxtTitle { get; private set; }
		public TextView TxtCountry { get; private set; }
		public TextView MovieRelease { get; private set; }


		#endregion
	}

	public class MoviesAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}