using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
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
using IList = System.Collections.IList;

namespace PlayTube.Activities.SettingsPreferences.Adapters
{
	public class BlockedUsersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		public event EventHandler<BlockedUsersAdapterClickEventArgs> OnItemClick;
		public event EventHandler<BlockedUsersAdapterClickEventArgs> OnItemLongClick;
		private readonly Activity ActivityContext;
		public ObservableCollection<UserDataObject> BlockedUsersList = new ObservableCollection<UserDataObject>();

		public BlockedUsersAdapter(Activity context)
		{
			try
			{
				ActivityContext = context;
				HasStableIds = true;
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
				//Setup your layout here >> Style_BlockedUsersView
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_BlockedUsersView, parent, false);
				var vh = new BlockedUsersAdapterViewHolder(itemView, Click, LongClick);
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
				if (viewHolder is BlockedUsersAdapterViewHolder holder)
				{
					var item = BlockedUsersList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

						string name = Methods.FunString.DecodeString(item.Name);
						holder.UserName.Text = Methods.FunString.SubStringCutOf(name, 25);
						holder.TxTuserText.Text = item.SubscribeCount + " " + ActivityContext.GetText(Resource.String.Lbl_Subscribers);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		public override int ItemCount => BlockedUsersList?.Count ?? 0;


		public UserDataObject GetItem(int position)
		{
			return BlockedUsersList[position];
		}

		public override long GetItemId(int position)
		{
			try
			{
				return int.Parse(BlockedUsersList[position].Id);
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

		void Click(BlockedUsersAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
		void LongClick(BlockedUsersAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);


		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is BlockedUsersAdapterViewHolder viewHolder)
				{
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.ImageUser);
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
				var item = BlockedUsersList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				if (item.Avatar != "")
				{
					d.Add(item.Avatar);
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

	public class BlockedUsersAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }

		public ImageView ImageUser { get; private set; }


		public TextView UserName { get; private set; }
		public TextView TxTuserText { get; private set; }

		public AppCompatButton BtnBlockControl { get; private set; }

		#endregion

		public BlockedUsersAdapterViewHolder(View itemView, Action<BlockedUsersAdapterClickEventArgs> clickListener, Action<BlockedUsersAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				//Get values
				ImageUser = (ImageView)MainView.FindViewById(Resource.Id.ImageUser);
				UserName = (TextView)MainView.FindViewById(Resource.Id.userName);
				TxTuserText = (TextView)MainView.FindViewById(Resource.Id.userText);

				BtnBlockControl = (AppCompatButton)MainView.FindViewById(Resource.Id.btn_block_control);

				//Event
				BtnBlockControl.Click += (sender, e) => clickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class BlockedUsersAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}