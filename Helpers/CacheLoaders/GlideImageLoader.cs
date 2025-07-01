using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Renderscripts;
using Android.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Engine.BitmapRecycle;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Load.Resource.Drawable;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Lang;
using Java.Security;
using PlayTube.Helpers.Utils;
using System.Linq;
using Exception = System.Exception;

namespace PlayTube.Helpers.CacheLoaders
{
	public enum ImageStyle
	{
		CenterCrop, CircleCrop, RoundedCrop, FitCenter, CircleCropWithBorder, Blur, BlurRounded, PaletteBitmapColor
	}

	public enum ImagePlaceholders
	{
		Color, Drawable, DrawableUser
	}

	public static class GlideImageLoader
	{
		public static void LoadImage(Activity activity, string imageUri, ImageView image, ImageStyle style, ImagePlaceholders imagePlaceholders, bool compress = false, RequestOptions options = null)
		{
			try
			{
				if (string.IsNullOrEmpty(imageUri) || string.IsNullOrWhiteSpace(imageUri) || image == null || activity?.IsDestroyed != false)
					return;

				char last = imageUri.Last();
				if (last.Equals(' '))
				{
					imageUri = imageUri.Remove(imageUri.Length - 1, 1);
				}

				var newImage = Glide.With(activity.BaseContext);

				options ??= GetOptions(style, imagePlaceholders);

				switch (compress)
				{
					case true when style != ImageStyle.RoundedCrop:
						{
							if (imageUri.Contains("avatar") || imageUri.Contains("Avatar"))
								options.Override(AppSettings.AvatarSize);
							else if (imageUri.Contains("gif"))
								options.Override(AppSettings.ImageSize);
							else
								options.Override(AppSettings.ImageSize);
							break;
						}
				}

				switch (compress)
				{
					case true:
						options.Override(AppSettings.ImageSize);
						break;
				}

				if (style is ImageStyle.Blur or ImageStyle.BlurRounded)
				{
					newImage.Load(imageUri).Apply(options)
						.Transition(DrawableTransitionOptions.WithCrossFade())
						.Transform(new BlurTransformation(activity), new CenterCrop())
						.Override(150)
						.Into(image);
					return;
				}

				//if (style == ImageStyle.PaletteBitmapColor)
				//{
				//    newImage.Load(imageUri).Apply(options) 
				//        .Into(new ColorGenerate(activity ,image));
				//     return;
				//}

				if (imageUri.Contains("no_profile_image") || imageUri.Contains("blackdefault") || imageUri.Contains("no_profile_image_circle")
					|| imageUri.Contains("ImagePlacholder") || imageUri.Contains("ImagePlacholder_circle")
					|| imageUri.Contains("Image_File") || imageUri.Contains("Audio_File") || imageUri.Contains("addImage") || imageUri.Contains("d-group") || imageUri.Contains("d-page")
					|| imageUri.Contains("d-cover") || imageUri.Contains("d-avatar") || imageUri.Contains("user_anonymous"))
				{
					if (imageUri.Contains("no_profile_image_circle"))
						newImage.Load(Resource.Drawable.no_profile_image_circle).Apply(options).Into(image);
					else if (imageUri.Contains("no_profile_image") || imageUri.Contains("d-avatar"))
						newImage.Load(Resource.Drawable.no_profile_image).Apply(options).Into(image);
					else if (imageUri.Contains("ImagePlacholder"))
						newImage.Load(Resource.Drawable.ImagePlacholder).Apply(options).Into(image);
					else if (imageUri.Contains("ImagePlacholder_circle"))
						newImage.Load(Resource.Drawable.ImagePlacholder_circle).Apply(options).Into(image);
					else if (imageUri.Contains("blackdefault"))
						newImage.Load(Resource.Drawable.blackdefault).Apply(options).Into(image);
				}
				else if (!string.IsNullOrEmpty(imageUri) && imageUri.Contains("http"))
				{
					newImage.Load(imageUri).Apply(options).Into(image);
				}
				else if (!string.IsNullOrEmpty(imageUri) && (imageUri.Contains("file://") || imageUri.Contains("content://") || imageUri.Contains("storage") || imageUri.Contains("/data/user/0/")))
				{
					try
					{
						File file2 = new File(imageUri);
						if (!System.IO.File.Exists(file2.Path))
						{
							Glide.With(activity.BaseContext).Load(file2).Apply(options).Into(image);
						}
						else
						{
							var photoUri = FileProvider.GetUriForFile(activity, activity.PackageName + ".fileprovider", file2);
							Glide.With(activity.BaseContext).Load(photoUri).Apply(options).Into(image);
						}
					}
					catch
					{
						Glide.With(activity.BaseContext).Load(imageUri).Apply(options).Into(image);
					}
				}
				else
				{
					newImage.Load(Resource.Drawable.no_profile_image).Apply(options).Into(image);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static RequestOptions GetOptions(ImageStyle style, ImagePlaceholders imagePlaceholders)
		{
			try
			{
				RequestOptions options = new RequestOptions();

				switch (style)
				{
					case ImageStyle.Blur:
					case ImageStyle.CenterCrop:
						options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
							.CenterCrop()
							.SetPriority(Priority.High)
							.SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).AutoClone());
						break;
					case ImageStyle.FitCenter:
						options = new RequestOptions().Apply(RequestOptions.CenterCropTransform().AutoClone()
							.FitCenter()
							.SetPriority(Priority.High)
							.SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All));
						break;
					case ImageStyle.CircleCrop:
						options = new RequestOptions().Apply(RequestOptions.CircleCropTransform().AutoClone()
							.CenterCrop().CircleCrop()
							.SetPriority(Priority.High)
							.SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All));
						break;
					case ImageStyle.CircleCropWithBorder:
						options = new RequestOptions().Apply(RequestOptions.CircleCropTransform().AutoClone()
							.CenterCrop().CircleCrop()
							.Transform(new GlideCircleWithBorder(1, Color.Black))
							.SetPriority(Priority.High)
							.SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All));
						break;
					case ImageStyle.BlurRounded:
					case ImageStyle.RoundedCrop:
						options = new RequestOptions().Apply(RequestOptions.CircleCropTransform().AutoClone()
							.CenterCrop()
							.Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(20)))
							.SetPriority(Priority.High)
							.SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All));
						break;
					default:
						options.CenterCrop();
						break;
				}

				if (imagePlaceholders == ImagePlaceholders.Color)
				{
					var color = Methods.FunString.RandomColor().Item1;
					options.Placeholder(new ColorDrawable(Color.ParseColor(color))).Fallback(new ColorDrawable(Color.ParseColor(color)));
				}
				else if (imagePlaceholders == ImagePlaceholders.Drawable)
				{
					if (style is ImageStyle.CircleCrop or ImageStyle.CircleCropWithBorder)
						options.Placeholder(Resource.Drawable.ImagePlacholder_circle).Fallback(Resource.Drawable.ImagePlacholder_circle);
					else
						options.Placeholder(Resource.Drawable.ImagePlacholder).Fallback(Resource.Drawable.ImagePlacholder);
				}
				else if (imagePlaceholders == ImagePlaceholders.DrawableUser)
				{
					if (style is ImageStyle.CircleCrop or ImageStyle.CircleCropWithBorder)
						options.Placeholder(Resource.Drawable.no_profile_image_circle).Fallback(Resource.Drawable.no_profile_image_circle);
					else
						options.Placeholder(Resource.Drawable.no_profile_image).Fallback(Resource.Drawable.no_profile_image);
				}

				return options;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return new RequestOptions().CenterCrop();
			}
		}

		public static RequestBuilder GetPreLoadRequestBuilder(Activity activityContext, string url, ImageStyle style)
		{
			try
			{
				if (url == null || string.IsNullOrEmpty(url))
					return null;

				var options = GetOptions(style, ImagePlaceholders.Drawable);

				if (url.Contains("avatar"))
					options.CircleCrop();

				if (url.Contains("avatar"))
				{
					options.Override(AppSettings.AvatarSize);
				}
				else if (url.Contains("gif"))
				{
					options.Override(AppSettings.ImageSize);
				}
				else
				{
					options.Override(AppSettings.ImageSize);
				}

				options.SetDiskCacheStrategy(DiskCacheStrategy.All);

				return Glide.With(activityContext.BaseContext)
					.Load(url)
					.Apply(options);

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

	}

	public class GlideCircleWithBorder : BitmapTransformation
	{
		private readonly Paint MBorderPaint;
		private readonly float MBorderWidth;

		public GlideCircleWithBorder(int borderWidth, Color borderColor)
		{
			try
			{
				if (Resources.System?.DisplayMetrics != null)
					MBorderWidth = Resources.System.DisplayMetrics.Density * borderWidth;

				MBorderPaint = new Paint { Dither = true, AntiAlias = true, Color = borderColor };
				MBorderPaint.SetStyle(Paint.Style.Stroke);
				MBorderPaint.StrokeWidth = MBorderWidth;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		protected override Bitmap Transform(IBitmapPool pool, Bitmap toTransform, int outWidth, int outHeight)
		{
			return CircleCrop(pool, toTransform);
		}

		public override void UpdateDiskCacheKey(MessageDigest p0)
		{

		}

		private Bitmap CircleCrop(IBitmapPool pool, Bitmap source)
		{
			try
			{
				switch (source)
				{
					case null:
						return null;
				}
				int size = (int)(Math.Min(source.Width, source.Height) - MBorderWidth / 2);
				int x = (source.Width - size) / 2;
				int y = (source.Height - size) / 2;
				Bitmap squared = Bitmap.CreateBitmap(source, x, y, size, size);
				Bitmap result = pool.Get(size, size, Bitmap.Config.Argb8888);
				switch (result)
				{
					case null:
						result = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
						break;
				}
				//Create a brush Canvas Manually draw a border
				Canvas canvas = new Canvas(result);
				Paint paint = new Paint();
				paint.SetShader(new BitmapShader(squared, Shader.TileMode.Clamp, Shader.TileMode.Clamp));
				paint.AntiAlias = true;
				float r = size / 2f;
				canvas.DrawCircle(r, r, r, paint);
				if (MBorderPaint != null)
				{
					float borderRadius = r - MBorderWidth / 2;
					canvas.DrawCircle(r, r, borderRadius, MBorderPaint);
				}
				return result;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}
	}

	public class BlurTransformation : BitmapTransformation
	{
		private readonly RenderScript RenderScript;

		public BlurTransformation(Context context)
		{
			RenderScript = RenderScript.Create(context);
		}

		protected override Bitmap Transform(IBitmapPool pool, Bitmap toTransform, int outWidth, int outHeight)
		{
			Bitmap blurredBitmap = toTransform.Copy(Bitmap.Config.Argb8888, true);

			Bitmap outputBitmap = Bitmap.CreateBitmap(blurredBitmap);

			Allocation tmpIn = Allocation.CreateFromBitmap(RenderScript, blurredBitmap);
			Allocation tmpOut = Allocation.CreateFromBitmap(RenderScript, outputBitmap);
			//Intrinsic Gausian blur filter
			ScriptIntrinsicBlur theIntrinsic = ScriptIntrinsicBlur.Create(RenderScript, Element.U8_4(RenderScript));
			theIntrinsic.SetRadius(25);
			theIntrinsic.SetInput(tmpIn);
			theIntrinsic.ForEach(tmpOut);
			tmpOut.CopyTo(outputBitmap);
			return outputBitmap;
		}

		public override void UpdateDiskCacheKey(MessageDigest p0)
		{

		}
	}

}