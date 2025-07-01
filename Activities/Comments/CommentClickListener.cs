using Android.App;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Dialog;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Comment;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PlayTube.Activities.Comments
{
	public class CommentClickListener : Java.Lang.Object, IDialogListCallBack, IDialogInputCallBack
	{
		private readonly Activity Context;
		private readonly TabbedMainActivity MainContext;
		private CommentDataObject CommentObject;
		private ReplyDataObject ReplyObject;
		private readonly string TypeClass;

		public CommentClickListener(Activity context, string typeClass)
		{
			Context = context;
			MainContext = TabbedMainActivity.GetInstance();
			TypeClass = typeClass;
		}

		public void MoreCommentPostClick(CommentDataObject item)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					CommentObject = item;

					var arrayAdapter = new List<string>();
					var dialogList = new MaterialAlertDialogBuilder(MainContext);

					arrayAdapter.Add(Context.GetString(Resource.String.Lbl_CopeText));

					if (CommentObject != null && (CommentObject.IsCommentOwner || CommentObject?.CommentUserData?.Id == UserDetails.UserId))
					{
						arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Edit));
						arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Delete));
					}

					dialogList.SetTitle(Context.GetString(Resource.String.Lbl_More));
					dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
					dialogList.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

					dialogList.Show();
				}
				else
				{
					Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Event Menu >> Delete Comment
		private void DeleteCommentEvent(CommentDataObject item)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					CommentObject = item;

					var dialog = new MaterialAlertDialogBuilder(MainContext);
					dialog.SetTitle(MainContext.GetText(Resource.String.Lbl_DeleteComment));
					dialog.SetMessage(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
					dialog.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Yes), (sender, args) =>
					{
						MainContext?.RunOnUiThread(() =>
						{
							try
							{
								switch (TypeClass)
								{
									case "Comment":
									case "CommentShort":
										{
											//TypeClass
											var adapterGlobal = MainContext?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter;
											var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
											if (dataGlobal != null)
											{
												var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
												if (index > -1)
												{
													adapterGlobal.CommentList.RemoveAt(index);
													adapterGlobal.NotifyItemRemoved(index);
												}
											}

											var adapterGlobal1 = GlobalPlayerActivity.GetInstance()?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter;
											var dataGlobal1 = adapterGlobal1?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
											if (dataGlobal1 != null)
											{
												var index = adapterGlobal1.CommentList.IndexOf(dataGlobal1);
												if (index > -1)
												{
													adapterGlobal1.CommentList.RemoveAt(index);
													adapterGlobal1.NotifyItemRemoved(index);
												}
											}

											PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.DeleteCommentAsync(CommentObject.Id.ToString()) });
											break;
										}
								}

								Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short)?.Show();
							}
							catch (Exception e)
							{
								Methods.DisplayReportResultTrack(e);
							}
						});

					});
					dialog.SetNegativeButton(MainContext.GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

					dialog.Show();
				}
				else
				{
					Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Event Menu >> Edit Comment
		private void EditCommentEvent(CommentDataObject item)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					CommentObject = item;

					var dialog = new MaterialAlertDialogBuilder(MainContext);

					dialog.SetTitle(Resource.String.Lbl_Edit);

					EditText input = new EditText(MainContext);
					input.SetHint(Resource.String.Lbl_Write_comment);
					input.InputType = InputTypes.TextFlagImeMultiLine;
					LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
					input.LayoutParameters = lp;

					dialog.SetView(input);

					dialog.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Update), new MaterialDialogUtils(input, this));
					dialog.SetNegativeButton(MainContext.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

					dialog.Show();
				}
				else
				{
					Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void MoreReplyPostClick(ReplyDataObject item)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					ReplyObject = item;

					var arrayAdapter = new List<string>();
					var dialogList = new MaterialAlertDialogBuilder(MainContext);

					arrayAdapter.Add(Context.GetString(Resource.String.Lbl_CopeText));

					if (ReplyObject != null && (ReplyObject.IsReplyOwner || ReplyObject?.ReplyUserData?.Id == UserDetails.UserId))
					{
						arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Edit));
						arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Delete));
					}

					dialogList.SetTitle(Context.GetString(Resource.String.Lbl_More));
					dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
					dialogList.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

					dialogList.Show();
				}
				else
				{
					Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Event Menu >> Delete Reply
		private void DeleteReplyEvent(ReplyDataObject item)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					ReplyObject = item;

					var dialog = new MaterialAlertDialogBuilder(MainContext);
					dialog.SetTitle(MainContext.GetText(Resource.String.Lbl_DeleteComment));
					dialog.SetMessage(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
					dialog.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Yes), (sender, args) =>
					{
						MainContext?.RunOnUiThread(() =>
						{
							try
							{
								switch (TypeClass)
								{
									case "Reply":
										{
											//TypeClass
											var adapterGlobal = ReplyCommentBottomSheet.GetInstance()?.ReplyAdapter;
											var dataGlobal = adapterGlobal?.ReplyList?.FirstOrDefault(a => a.Id == ReplyObject?.Id);
											if (dataGlobal != null)
											{
												var index = adapterGlobal.ReplyList.IndexOf(dataGlobal);
												if (index > -1)
												{
													adapterGlobal.ReplyList.RemoveAt(index);
													adapterGlobal.NotifyItemRemoved(index);
												}
											}

											PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.DeleteReplyAsync(ReplyObject.Id.ToString()) });
											break;
										}
								}

								Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short)?.Show();
							}
							catch (Exception e)
							{
								Methods.DisplayReportResultTrack(e);
							}
						});

					});
					dialog.SetNegativeButton(MainContext.GetText(Resource.String.Lbl_No), new MaterialDialogUtils());
					dialog.Show();
				}
				else
				{
					Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Event Menu >> Edit Reply
		private void EditReplyEvent(ReplyDataObject item)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					ReplyObject = item;

					var dialog = new MaterialAlertDialogBuilder(MainContext);

					dialog.SetTitle(Resource.String.Lbl_Edit);

					EditText input = new EditText(MainContext);
					input.SetHint(Resource.String.Lbl_Write_comment);
					input.Text = Methods.FunString.DecodeString(item.Text);
					input.InputType = InputTypes.TextFlagImeMultiLine;
					LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
					input.LayoutParameters = lp;

					dialog.SetView(input);

					dialog.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Update), new MaterialDialogUtils(input, this));
					dialog.SetNegativeButton(MainContext.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
					dialog.Show();
				}
				else
				{
					Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#region MaterialDialog

		public void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				string text = itemString;
				if (text == Context.GetString(Resource.String.Lbl_CopeText))
				{
					switch (TypeClass)
					{
						case "Comment":
						case "CommentShort":
							Methods.CopyToClipboard(MainContext, Methods.FunString.DecodeString(CommentObject.Text));
							break;
						case "Reply":
							Methods.CopyToClipboard(MainContext, Methods.FunString.DecodeString(ReplyObject.Text));
							break;
					}
				}
				else if (text == Context.GetString(Resource.String.Lbl_Edit))
				{
					switch (TypeClass)
					{
						case "Comment":
						case "CommentShort":
							EditCommentEvent(CommentObject);
							break;
						case "Reply":
							EditReplyEvent(ReplyObject);
							break;
					}
				}
				else if (text == Context.GetString(Resource.String.Lbl_Delete))
				{
					switch (TypeClass)
					{
						case "Comment":
						case "CommentShort":
							DeleteCommentEvent(CommentObject);
							break;
						case "Reply":
							DeleteReplyEvent(ReplyObject);
							break;
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnInput(IDialogInterface dialog, string input)
		{
			try
			{
				if (input.Length > 0)
				{
					if (!Methods.CheckConnectivity())
					{
						Toast.MakeText(MainContext, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
					else
					{
						switch (TypeClass)
						{
							case "Comment":
							case "CommentShort":
								{
									//TypeClass
									var adapterGlobal = MainContext?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter;
									var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
									if (dataGlobal != null)
									{
										dataGlobal.Text = input;
										var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
										if (index > -1)
										{
											adapterGlobal.NotifyItemChanged(index);
										}
									}

									var adapterGlobal1 = GlobalPlayerActivity.GetInstance()?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter;
									var dataGlobal1 = adapterGlobal1?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
									if (dataGlobal1 != null)
									{
										dataGlobal1.Text = input;
										var index = adapterGlobal1.CommentList.IndexOf(dataGlobal1);
										if (index > -1)
										{
											adapterGlobal1.NotifyItemChanged(index);
										}
									}
									PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.EditCommentAsync(CommentObject.Id.ToString(), input) });
									break;
								}
							case "Reply":
								{
									//TypeClass
									var adapterGlobal = ReplyCommentBottomSheet.GetInstance()?.ReplyAdapter;
									var dataGlobal = adapterGlobal?.ReplyList?.FirstOrDefault(a => a.Id == ReplyObject?.Id);
									if (dataGlobal != null)
									{
										dataGlobal.Text = input;
										var index = adapterGlobal.ReplyList.IndexOf(dataGlobal);
										if (index > -1)
										{
											adapterGlobal.NotifyItemChanged(index);
										}
									}

									PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.EditCommentReplyAsync(ReplyObject.Id.ToString(), input) });
									break;
								}
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion MaterialDialog 
	}
}