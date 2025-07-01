using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Helpers.Models
{
	public class TextDecorator
	{
		public string Content;
		public SpannableString DecoratedContent;
		public int Flags;

		public void Build(TextView textHolder, SpannableString decoratedContent)
		{
			try
			{
				textHolder.SetText(decoratedContent, TextView.BufferType.Spannable);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetTextColor(string texts, string color)
		{
			try
			{
				string content = texts;
				if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
					return;

				var indexFrom = content.IndexOf(texts, StringComparison.Ordinal);
				indexFrom = indexFrom switch
				{
					<= -1 => 0,
					_ => indexFrom
				};

				var indexLast = indexFrom + texts.Length;
				indexLast = indexLast switch
				{
					<= -1 => 0,
					_ => indexLast
				};

				DecoratedContent.SetSpan(new ForegroundColorSpan(Color.ParseColor(color)), indexFrom, indexLast, SpanTypes.ExclusiveExclusive);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//public void SetRelativeSize(string texts, float proportion)
		//{
		//    try
		//    {
		//        DecoratedContent.SetSpan(new RelativeSizeSpan(proportion), Content.IndexOf(texts, StringComparison.Ordinal), Content.IndexOf(texts, StringComparison.Ordinal) + (texts.Length), SpanTypes.ExclusiveExclusive);
		//    }
		//    catch (Exception e)
		//    {
		//        Methods.DisplayReportResultTrack(e);
		//    } 
		//} 

		//public void SetTextStyle(string texts, TypefaceStyle style)
		//{
		//    try
		//    {
		//        DecoratedContent.SetSpan(new StyleSpan(style), Content.IndexOf(texts, StringComparison.Ordinal), Content.IndexOf(texts, StringComparison.Ordinal) + (texts.Length), SpanTypes.ExclusiveExclusive);
		//    }
		//    catch (Exception e)
		//    {
		//        Methods.DisplayReportResultTrack(e);
		//    }
		//}

		//public void MakeTextClickable(string texts)
		//{
		//    foreach (var text in texts.Where(text => Content.Contains(text)))
		//    {
		//        var index = Content.IndexOf(text);
		//        DecoratedContent.SetSpan(new ClickSpanClass(), index, index + text.ToString().Length, SpanTypes.ExclusiveExclusive);
		//    }

		//    //textView.setMovementMethod(LinkMovementMethod.getInstance());
		//}

		public class ClickSpanClass : ClickableSpan
		{
			public override void OnClick(View widget)
			{

			}
		}
	}
}