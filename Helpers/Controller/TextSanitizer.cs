//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.Core.Content;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.SuperTextLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayTube.Helpers.Controller
{
	public class TextSanitizer : StTools.IXAutoLinkOnClickListener
	{
		private readonly TabbedMainActivity Context;
		private readonly SuperTextView SuperTextView;
		private readonly Activity Activity;

		public TextSanitizer(SuperTextView linkTextView, Activity activity, TabbedMainActivity contextTabbed = null)
		{
			try
			{
				SuperTextView = linkTextView;
				Activity = activity;
				Context = contextTabbed;

				SuperTextView.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void Load(string text, string position = "Left")
		{
			try
			{
				SuperTextView.AddAutoLinkMode(new[] { StTools.XAutoLinkMode.ModePhone, StTools.XAutoLinkMode.ModeEmail, StTools.XAutoLinkMode.ModeHashTag, StTools.XAutoLinkMode.ModeUrl, StTools.XAutoLinkMode.ModeMention });
				if (position == "Right" || position == "right")
				{
					SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModePhone_color)));
					SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeEmail_color)));
					SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeHashtag_color)));
					SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color)));
					SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeMention_color)));
					SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color)));
					SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));
				}
				else
				{
					SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.Left_ModePhone_color)));
					SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.Left_ModeEmail_color)));
					SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.Left_ModeHashtag_color)));
					SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.Left_ModeUrl_color)));
					SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.Left_ModeMention_color)));
					SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.Left_ModeUrl_color)));
					SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));
				}

				var textt = text.Split('/');
				if (textt.Length > 1)
				{
					SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.Left_ModeUrl_color)));
					SuperTextView.SetCustomRegex(@"\b(" + textt.LastOrDefault() + @")\b");
				}

				string laststring = text.Replace(" /", " ");
				if (!string.IsNullOrEmpty(laststring))
					SuperTextView.SetText(laststring, TextView.BufferType.Spannable);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void AutoLinkTextClick(StTools.XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData)
		{
			try
			{
				var typetext = Methods.FunString.Check_Regex(matchedText.Replace(" ", "").Replace("\n", "").Replace("\n", ""));
				if (typetext == "Email" || autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
				{
					Methods.App.SendEmail(Activity, matchedText.Replace(" ", "").Replace("\n", ""));
				}
				else if (typetext == "Website" || autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
				{
					string url = matchedText.Replace(" ", "").Replace("\n", "");
					if (!matchedText.Contains("http"))
					{
						url = "http://" + matchedText.Replace(" ", "").Replace("\n", "");
					}

					//var intent = new Intent(Activity, typeof(LocalWebViewActivity));
					//intent.PutExtra("URL", url);
					//intent.PutExtra("Type", url);
					//Activity.StartActivity(intent);
					new IntentController(Context).OpenBrowserFromApp(url);
				}
				else if (typetext == "Hashtag" || autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
				{
					Bundle bundle = new Bundle();
					bundle.PutString("Key", matchedText.Replace("#", ""));
					VideosByKeyFragment videoViewerFragment = new VideosByKeyFragment
					{
						Arguments = bundle
					};
					Context?.FragmentBottomNavigator?.DisplayFragment(videoViewerFragment);
				}
				else if (typetext == "Mention" || autoLinkMode == StTools.XAutoLinkMode.ModeMention)
				{
					Bundle bundle = new Bundle();
					bundle.PutString("Key", matchedText.Replace("@", ""));
					VideosByKeyFragment videoViewerFragment = new VideosByKeyFragment
					{
						Arguments = bundle
					};
					Context?.FragmentBottomNavigator?.DisplayFragment(videoViewerFragment);
				}
				else if (typetext == "Number" || autoLinkMode == StTools.XAutoLinkMode.ModePhone)
				{
					Methods.App.SaveContacts(Activity, matchedText.Replace(" ", "").Replace("\n", ""), "", "2");
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}

		}
	}
}