using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Adapters;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlayTube.Helpers.Controller
{
	public class CategoriesController
	{
		//Categories Communities Local Custom
		public static ObservableCollection<Classes.Category> ListCategories = new ObservableCollection<Classes.Category>();
		public static ObservableCollection<Classes.Category> ListCategoriesMovies = new ObservableCollection<Classes.Category>();

		public static string GetCategoryName(VideoDataObject videoObject)
		{
			try
			{
				if (videoObject == null)
					return Application.Context.GetString(Resource.String.Lbl_Unknown);

				if (videoObject.IsMovie == "1")
				{
					string name = Methods.FunString.DecodeString(ListCategoriesMovies?.FirstOrDefault(a => a.Id == videoObject.CategoryId)?.Name);
					if (!string.IsNullOrEmpty(name))
						return name;
				}
				else if (!string.IsNullOrEmpty(videoObject.CategoryId) && !string.IsNullOrEmpty(videoObject.SubCategory) && videoObject.SubCategory != "0")
				{
					var category = ListCategories?.FirstOrDefault(a => a.Id == videoObject.CategoryId);
					if (category != null)
					{
						string name = Methods.FunString.DecodeString(category.SubList.FirstOrDefault(a => a.Id == videoObject.CategoryId)?.Name);
						if (!string.IsNullOrEmpty(name))
							return name;
					}
				}
				else
				{
					string name = Methods.FunString.DecodeString(ListCategories?.FirstOrDefault(a => a.Id == videoObject.CategoryId)?.Name);
					if (!string.IsNullOrEmpty(name))
						return name;
				}

				return Application.Context.GetString(Resource.String.Lbl_Unknown);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return Application.Context.GetString(Resource.String.Lbl_Unknown);
			}
		}

		public static int GetImageCategory(string name)
		{
			if (name.Contains("Film & Animation"))
			{
				return Resource.Drawable.icon_movies_vector;
			}

			if (name.Contains("Music"))
			{
				return Resource.Drawable.icon_cat_music_vector;
			}

			if (name.Contains("Pets & Animals"))
			{
				return Resource.Drawable.icon_cat_animals_vector;
			}

			if (name.Contains("Sports"))
			{
				return Resource.Drawable.icon_cat_sport_vector;
			}

			if (name.Contains("Travel & Events"))
			{
				return Resource.Drawable.icon_cat_travel_vector;
			}

			if (name.Contains("Gaming"))
			{
				return Resource.Drawable.icon_cat_gaming_vector;
			}

			if (name.Contains("People & Blogs"))
			{
				return Resource.Drawable.icon_cat_people_vector;
			}

			if (name.Contains("Comedy"))
			{
				return Resource.Drawable.icon_cat_comedy_vector;
			}

			if (name.Contains("Entertainment"))
			{
				return Resource.Drawable.icon_cat_entertainment_vector;
			}

			if (name.Contains("News & Politics"))
			{
				return Resource.Drawable.icon_cat_news_vector;
			}

			if (name.Contains("How-to & Style"))
			{
				return Resource.Drawable.icon_cat_help_vector;
			}

			if (name.Contains("Non-profits & Activism"))
			{
				return Resource.Drawable.icon_cat_activism_vector;
			}

			return Resource.Drawable.icon_cat_other_vector;
		}

		/// <summary>
		/// When the category name is changed to a language or not a name, it gives one symbol for it, and it must be changed by:
		/// https://fontawesome.com/icons
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string GetIconCategory(string name)
		{
			if (name.Contains("Film & Animation"))
			{
				return FontAwesomeIcon.Film;
			}

			if (name.Contains("Cars & Vehicles"))
			{
				return FontAwesomeIcon.Car;
			}

			if (name.Contains("Music"))
			{
				return FontAwesomeIcon.Music;
			}

			if (name.Contains("Pets & Animals"))
			{
				return FontAwesomeIcon.Crow;
			}

			if (name.Contains("Sports"))
			{
				return FontAwesomeIcon.VolleyballBall;
			}

			if (name.Contains("Travel & Events"))
			{
				return FontAwesomeIcon.Route;
			}

			if (name.Contains("Gaming"))
			{
				return FontAwesomeIcon.Gamepad;
			}

			if (name.Contains("People & Blogs"))
			{
				return FontAwesomeIcon.Blog;
			}

			if (name.Contains("Comedy"))
			{
				return FontAwesomeIcon.TheaterMasks;
			}

			if (name.Contains("Entertainment"))
			{
				return FontAwesomeIcon.SmileWink;
			}

			if (name.Contains("News & Politics"))
			{
				return FontAwesomeIcon.Newspaper;
			}

			if (name.Contains("How-to & Style"))
			{
				return FontAwesomeIcon.Tshirt;
			}

			if (name.Contains("Non-profits & Activism"))
			{
				return FontAwesomeIcon.Running;
			}

			return FontAwesomeIcon.FeatherAlt;
		}

		#region Dialog 

		private static Dialog DialogSelectCategories;
		private static CategoryAdapter MCatAdapter;

		public static void DisplayCreditWindow(Activity activityContext)
		{
			try
			{
				DialogSelectCategories = new Dialog(activityContext, Resource.Style.MyTheme);
				DialogSelectCategories.SetContentView(Resource.Layout.DialogSelectCategoriesLayout);

				var iconClose = DialogSelectCategories.FindViewById<ImageView>(Resource.Id.iconClose);
				iconClose.Click += IconCloseOnClick;

				var recyclerView = DialogSelectCategories.FindViewById<RecyclerView>(Resource.Id.recyler);

				recyclerView.SetLayoutManager(new LinearLayoutManager(activityContext));
				MCatAdapter = new CategoryAdapter(activityContext)
				{
					CategoryList = new ObservableCollection<Classes.Category>()
				};
				MCatAdapter.ItemClick += MCatAdapterOnItemClick;
				recyclerView.SetAdapter(MCatAdapter);

				if (MCatAdapter.CategoryList.Count == 0 && CategoriesController.ListCategories.Count > 0)
				{
					MCatAdapter.CategoryList = CategoriesController.ListCategories;
					MCatAdapter.NotifyDataSetChanged();
				}

				DialogSelectCategories.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private static void IconCloseOnClick(object sender, EventArgs e)
		{
			try
			{
				DialogSelectCategories.Hide();
				DialogSelectCategories.Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private static void MCatAdapterOnItemClick(object sender, CategoryAdapterClickEventArgs e)
		{
			try
			{
				int position = e.Position;
				if (position > -1)
				{
					var item = MCatAdapter.GetItem(e.Position);
					if (item == null) return;

					Bundle bundle = new Bundle();
					bundle.PutString("CatId", item.Id);
					bundle.PutString("CatName", item.Name);

					VideosByCategoryFragment videoViewerFragment = new VideosByCategoryFragment
					{
						Arguments = bundle
					};

					TabbedMainActivity.GetInstance()?.FragmentBottomNavigator?.DisplayFragment(videoViewerFragment);

					DialogSelectCategories.Hide();
					DialogSelectCategories.Dismiss();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion


	}
}