using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.SuperTextLibrary;
using PlayTube.PlayTubeClient.Classes.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlayTube.Activities.Chat.Adapters
{
	public class UserMessagesAdapter : RecyclerView.Adapter
	{
		public readonly MessagesBoxActivity ActivityContext;
		public ObservableCollection<GetUserMessagesObject.Message> MessageList = new ObservableCollection<GetUserMessagesObject.Message>();
		public event EventHandler<UserMessagesAdapterClickEventArgs> ItemClick;
		public event EventHandler<UserMessagesAdapterClickEventArgs> ItemLongClick;

		public readonly SparseBooleanArray SelectedItems;
		public IOnClickListenerSelectedMessages OnClickListener;
		public int CurrentSelectedIdx = -1;

		public UserMessagesAdapter(MessagesBoxActivity context)
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

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> 
				var itemView = MessageList[viewType];
				if (itemView != null)
				{
					if (itemView.Position == "Right" || itemView.Position == "right")
					{
						View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_view, parent, false);
						TextViewHolder textViewHolder = new TextViewHolder(row, OnClick, OnLongClick, ActivityContext);
						return textViewHolder;
					}
					else
					{
						View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
						TextViewHolder textViewHolder = new TextViewHolder(row, OnClick, OnLongClick, ActivityContext);
						return textViewHolder;
					}
				}
				else
				{
					return null;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return null;
			}
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
		{
			try
			{
				int type = GetItemViewType(position);
				var item = MessageList[type];
				if (item != null)
				{
					if (viewHolder is TextViewHolder holder)
					{
						Initialize(holder, item);

						holder.LytParent.Activated = SelectedItems.Get(position, false);

						holder.LytParent.Click += delegate
						{
							try
							{
								if (OnClickListener == null) return;

								OnClickListener.OnItemClick(holder.LytParent, item, position);
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

								OnClickListener.OnItemLongClick(holder.LytParent, item, position);
							}
							catch (Exception e)
							{
								Methods.DisplayReportResultTrack(e);
							}
						};

						ToggleCheckedBackground(holder, position);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void SetOnClickListener(IOnClickListenerSelectedMessages onClickListener)
		{
			OnClickListener = onClickListener;
		}

		public void Initialize(TextViewHolder holder, GetUserMessagesObject.Message item)
		{
			try
			{
				holder.Time.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time));

				string text = Methods.FunString.DecodeString(item.Text);
				holder.TextSanitizerAutoLink.Load(text, item.Position);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override int ItemCount => MessageList?.Count ?? 0;

		// Function 
		public void Add(GetUserMessagesObject.Message item)
		{
			try
			{
				var check = MessageList.FirstOrDefault(a => a.Id == item.Id);
				if (check == null)
				{
					MessageList.Add(item);
					NotifyItemInserted(MessageList.IndexOf(MessageList.Last()));
					//Scroll Down >> 
					ActivityContext.ChatBoxRecylerView.ScrollToPosition(MessageList.IndexOf(MessageList.Last()));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void Insert(GetUserMessagesObject.Message message, string firstId)
		{
			try
			{
				var check = MessageList.FirstOrDefault(a => a.Id == message.Id);
				if (check == null)
				{
					MessageList.Insert(0, message);
					NotifyItemInserted(MessageList.IndexOf(MessageList.FirstOrDefault()));

					var index = MessageList.FirstOrDefault(a => a.Id == firstId);
					if (index != null)
					{
						NotifyItemChanged(MessageList.IndexOf(index));
						//Scroll Down >> 
						ActivityContext.ChatBoxRecylerView.ScrollToPosition(MessageList.IndexOf(index));
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void Update(GetUserMessagesObject.Message messages)
		{
			try
			{
				var checker = MessageList.FirstOrDefault(a => a.Id == messages.Id);
				if (checker != null)
				{
					checker.Id = messages.Id;
					checker.FromId = messages.FromId;
					checker.ToId = messages.ToId;
					checker.Text = messages.Text;
					checker.Seen = messages.Seen;
					checker.Time = messages.Time;
					checker.FromDeleted = messages.FromDeleted;
					checker.ToDeleted = messages.ToDeleted;
					checker.TextTime = messages.TextTime;
					checker.Position = messages.Position;

					ActivityContext?.RunOnUiThread(() =>
					{
						NotifyItemChanged(MessageList.IndexOf(checker));
						ActivityContext.ChatBoxRecylerView.ScrollToPosition(MessageList.IndexOf(MessageList.Last()));
					});
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
				MessageList.Clear();
				NotifyDataSetChanged();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void Remove(GetUserMessagesObject.Message users)
		{
			try
			{
				var index = MessageList.IndexOf(MessageList.FirstOrDefault(a => a.Id == users.Id));
				if (index != -1)
				{
					MessageList.Remove(users);
					NotifyItemRemoved(index);
					NotifyItemRangeRemoved(0, ItemCount);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public GetUserMessagesObject.Message GetItem(int position)
		{
			return MessageList[position];
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

		private void OnClick(UserMessagesAdapterClickEventArgs args)
		{
			ItemClick?.Invoke(this, args);
		}

		private void OnLongClick(UserMessagesAdapterClickEventArgs args)
		{
			ItemLongClick?.Invoke(this, args);
		}

		#region Toolbar & Selected

		private void ToggleCheckedBackground(TextViewHolder holder, int position)
		{
			try
			{
				if (SelectedItems.Get(position, false))
				{
					holder.MainView.SetBackgroundColor(Color.LightBlue);
					if (CurrentSelectedIdx == position) ResetCurrentItems();
				}
				else
				{
					holder.MainView.SetBackgroundColor(Color.Transparent);
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

		public void RemoveData(int position, GetUserMessagesObject.Message users)
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
	}

	public class UserMessagesAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }

		#endregion

		public UserMessagesAdapterViewHolder(View itemView, Action<UserMessagesAdapterClickEventArgs> clickListener,
			Action<UserMessagesAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				//Create an Event
				itemView.Click += (sender, e) => clickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class TextViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public LinearLayout LytParent { get; private set; }
		public TextView Time { get; private set; }
		public View MainView { get; private set; }
		public SuperTextView SuperTextView { get; set; }
		public TextSanitizer TextSanitizerAutoLink { get; set; }

		#endregion

		public TextViewHolder(View itemView, Action<UserMessagesAdapterClickEventArgs> clickListener, Action<UserMessagesAdapterClickEventArgs> longClickListener, Activity activity) : base(itemView)
		{
			try
			{
				MainView = itemView;

				LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
				SuperTextView = itemView.FindViewById<SuperTextView>(Resource.Id.active);
				Time = itemView.FindViewById<TextView>(Resource.Id.time);

				SuperTextView.SetTextIsSelectable(true);

				if (TextSanitizerAutoLink == null)
				{
					TextSanitizerAutoLink = new TextSanitizer(SuperTextView, activity);
				}

				itemView.Click += (sender, e) => clickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}

	public class UserMessagesAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}