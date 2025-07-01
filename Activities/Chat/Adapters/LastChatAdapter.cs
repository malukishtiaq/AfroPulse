using AmulyaKhare.TextDrawableLib;
using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace PlayTube.Activities.Chat.Adapters
{
	public class LastChatAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		private readonly Activity ActivityContext;

		public ObservableCollection<GetChatsObject.Data> UserList = new ObservableCollection<GetChatsObject.Data>();
		public event EventHandler<LastChatAdapterClickEventArgs> ItemClick;
		public event EventHandler<LastChatAdapterClickEventArgs> ItemLongClick;

		public IOnClickListenerSelected OnClickListener;
		public readonly SparseBooleanArray SelectedItems;
		public int CurrentSelectedIdx = -1;

		public LastChatAdapter(Activity context)
		{
			try
			{
				ActivityContext = context;
				SelectedItems = new SparseBooleanArray();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetOnClickListener(IOnClickListenerSelected onClickListener)
		{
			OnClickListener = onClickListener;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
		{
			try
			{
				if (viewHolder is LastChatAdapterViewHolder holder)
				{
					var item = UserList[position];
					if (item != null)
					{
						Initialize(holder, item);

						holder.LytParent.Activated = SelectedItems.Get(position, false);

						if (!holder.LytParent.HasOnClickListeners)
						{
							holder.LytParent.Click += delegate
							{
								try
								{
									if (OnClickListener == null) return;

									OnClickListener.OnItemClick(holder.MainView, item, position);
								}
								catch (Exception e)
								{
									Methods.DisplayReportResultTrack(e);
								}
							};

							holder.LytParent.LongClick += delegate
							{
								try
								{
									if (OnClickListener == null) return;

									OnClickListener.OnItemLongClick(holder.MainView, item, position);

								}
								catch (Exception e)
								{
									Methods.DisplayReportResultTrack(e);
								}
							};
						}

						ToggleCheckedIcon(holder, position);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void Initialize(LastChatAdapterViewHolder holder, GetChatsObject.Data item)
		{
			try
			{

				GlideImageLoader.LoadImage(ActivityContext, item.User?.Avatar, holder.ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

				string name = Methods.FunString.DecodeString(item.User?.Name);
				if (holder.TxtUsername.Text != name)
				{
					holder.TxtUsername.Text = name;
				}

				if (item.GetLastMessage != null)
				{
					string lastMessage = Methods.FunString.DecodeString(item.GetLastMessage.Text);
					if (holder.TxtLastMessages.Text != lastMessage)
					{
						holder.TxtLastMessages.Text = lastMessage;
					}

					//last seen time  
					holder.TxtTimestamp.Text = Methods.Time.ReplaceTime(item.GetLastMessage.TextTime);
				}

				if (Convert.ToInt32(item.GetCountSeen) <= 0)
				{
					holder.ImageColor.Visibility = ViewStates.Invisible;
				}
				else
				{
					var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(25).EndConfig().BuildRound(item.GetCountSeen, Color.ParseColor(AppSettings.MainColor));
					holder.ImageColor.SetImageDrawable(drawable);
					holder.ImageColor.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> Style_HContact_view
				var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_LastChatView, parent, false);
				var vh = new LastChatAdapterViewHolder(itemView, OnClick, OnLongClick);
				return vh;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return null;
			}
		}

		public override int ItemCount => UserList?.Count ?? 0;



		public void Add(GetChatsObject.Data user)
		{
			try
			{
				var check = UserList.FirstOrDefault(a => a.User.Id == user.User.Id);
				if (check == null)
				{
					UserList.Add(user);
					NotifyItemInserted(UserList.IndexOf(UserList.Last()));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void Move(GetChatsObject.Data users)
		{
			try
			{
				var data = UserList.FirstOrDefault(a => a.User.Id == users.User.Id);
				if (data != null)
				{
					var index = UserList.IndexOf(data);
					if (index > -1 && index != 0)
					{
						UserList.Move(index, 0);
						NotifyItemMoved(index, 0);
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void Insert(GetChatsObject.Data users)
		{
			try
			{
				UserList.Insert(0, users);
				NotifyItemInserted(0);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void Update(GetChatsObject.Data user)
		{
			try
			{
				var data = UserList.FirstOrDefault(a => a.User.Id == user.Id);
				if (data != null)
				{
					data.Id = user.Id;
					data.UserOne = user.UserOne;
					data.UserTwo = user.UserTwo;
					data.Time = user.Time;
					data.TextTime = user.TextTime;
					data.User = user.User;
					user.GetCountSeen = user.GetCountSeen;
					data.GetLastMessage = user.GetLastMessage;

					ActivityContext?.RunOnUiThread(() =>
					{
						NotifyItemChanged(UserList.IndexOf(data));
					});
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void Remove(GetChatsObject.Data users)
		{
			try
			{
				var index = UserList.IndexOf(UserList.FirstOrDefault(a => a.User.Id == users.User.Id));
				if (index != -1)
				{
					UserList.Remove(users);
					NotifyItemRemoved(index);
					NotifyItemRangeRemoved(0, ItemCount);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void BindEnd()
		{
			try
			{
				NotifyDataSetChanged();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void Clear()
		{
			try
			{

				ListUtils.ChatList.Clear();
				UserList.Clear();
				NotifyDataSetChanged();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#region Toolbar & Selected

		private void ToggleCheckedIcon(LastChatAdapterViewHolder holder, int position)
		{
			try
			{
				if (SelectedItems.Get(position, false))
				{
					holder.LytImage.Visibility = ViewStates.Gone;
					holder.LytChecked.Visibility = ViewStates.Visible;
					if (CurrentSelectedIdx == position) ResetCurrentItems();
				}
				else
				{
					holder.LytChecked.Visibility = ViewStates.Gone;
					holder.LytImage.Visibility = ViewStates.Visible;
					if (CurrentSelectedIdx == position) ResetCurrentItems();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}

		}

		private void ResetCurrentItems()
		{
			try
			{
				CurrentSelectedIdx = -1;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		public int GetSelectedItemCount()
		{
			return SelectedItems.Size();
		}

		public List<int> GetSelectedItems()
		{
			List<int> items = new List<int>(SelectedItems.Size());
			for (int i = 0; i < SelectedItems.Size(); i++)
			{
				items.Add(SelectedItems.KeyAt(i));
			}
			return items;
		}

		public void RemoveData(int position, GetChatsObject.Data users)
		{
			try
			{
				Remove(users);
				ResetCurrentItems();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void ClearSelections()
		{
			try
			{
				SelectedItems.Clear();
				NotifyDataSetChanged();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void ToggleSelection(int pos)
		{
			try
			{
				CurrentSelectedIdx = pos;
				if (SelectedItems.Get(pos, false))
				{
					SelectedItems.Delete(pos);
				}
				else
				{
					SelectedItems.Put(pos, true);
				}
				NotifyItemChanged(pos);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		#endregion

		public GetChatsObject.Data GetItem(int position)
		{
			return UserList[position];
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

		private void OnClick(LastChatAdapterClickEventArgs args)
		{
			ItemClick?.Invoke(this, args);
		}

		private void OnLongClick(LastChatAdapterClickEventArgs args)
		{
			ItemLongClick?.Invoke(this, args);
		}


		public IList GetPreloadItems(int p0)
		{
			try
			{
				var d = new List<string>();
				var item = UserList[p0];
				if (item == null)
					return Collections.SingletonList(p0);

				if (item.User?.Avatar != "")
				{
					d.Add(item.User?.Avatar);
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

		public RequestBuilder GetPreloadRequestBuilder(Object p0)
		{
			return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
		}
	}

	public class LastChatAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }

		public RelativeLayout LytParent { get; private set; }
		public TextView TxtUsername { get; private set; }
		public TextView TxtLastMessages { get; private set; }
		public TextView TxtTimestamp { get; private set; }
		public ImageView ImageAvatar { get; private set; }
		public ImageView ImageColor { get; private set; }

		public RelativeLayout LytChecked { get; private set; }
		public RelativeLayout LytImage { get; private set; }

		#endregion

		public LastChatAdapterViewHolder(View itemView, Action<LastChatAdapterClickEventArgs> clickListener, Action<LastChatAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				//Get values
				LytParent = (RelativeLayout)MainView.FindViewById(Resource.Id.main);
				TxtUsername = (TextView)MainView.FindViewById(Resource.Id.Txt_Username);
				TxtLastMessages = (TextView)MainView.FindViewById(Resource.Id.Txt_LastMessages);
				TxtTimestamp = (TextView)MainView.FindViewById(Resource.Id.Txt_timestamp);
				ImageAvatar = (ImageView)MainView.FindViewById(Resource.Id.ImageAvatar);

				ImageColor = (ImageView)MainView.FindViewById(Resource.Id.image_view);

				LytChecked = (RelativeLayout)MainView.FindViewById(Resource.Id.lyt_checked);
				LytImage = (RelativeLayout)MainView.FindViewById(Resource.Id.lyt_image);


				//Create an Event
				//itemView.Click += (sender, e) => clickListener(new LastChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				//itemView.LongClick += (sender, e) => longClickListener(new LastChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class LastChatAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}