using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PlayTube.Activities.Movies
{
	public class MoviesFilterDialogFragment : BottomSheetDialogFragment, IDialogListCallBack
	{
		#region Variables Basic

		private EditText TxtSearchTerm, TxtRelease, TxtCountry, TxtCategory;
		private TextView ResetButton;
		private RatingBar RatingBar;
		private AppCompatButton BtnApply;
		private string TypeDialog, CountryId, CategoryId;
		private readonly MoviesFragment ContextMovies;
		private string Rating;
		private readonly string[] ExperienceDate = Application.Context.Resources?.GetStringArray(Resource.Array.experience_date);

		#endregion

		public MoviesFilterDialogFragment(MoviesFragment moviesActivity)
		{
			ContextMovies = moviesActivity;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
				// clone the inflater using the ContextThemeWrapper
				LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

				View view = localInflater?.Inflate(Resource.Layout.MoviesFilterLayout, container, false);

				InitComponent(view);

				ResetButton.Click += ResetButtonOnClick;
				BtnApply.Click += BtnApplyOnClick;
				TxtRelease.Touch += TxtReleaseOnTouch;
				TxtCountry.Touch += TxtCountryOnTouch;
				TxtCategory.Touch += TxtCategoryOnTouch;
				RatingBar.RatingBarChange += RatingBarOnRatingBarChange;

				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		private void RatingBarOnRatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
		{
			try
			{
				RatingBar.Rating = e.Rating;
				Rating = e.Rating.ToString(CultureInfo.InvariantCulture);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void OnLowMemory()
		{
			try
			{
				GC.Collect(GC.MaxGeneration);
				base.OnLowMemory();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				TxtSearchTerm = view.FindViewById<EditText>(Resource.Id.SearchTermEditText);
				RatingBar = view.FindViewById<RatingBar>(Resource.Id.ratingBar);
				TxtRelease = view.FindViewById<EditText>(Resource.Id.ReleaseEditText);
				TxtCountry = view.FindViewById<EditText>(Resource.Id.CountryEditText);
				TxtCategory = view.FindViewById<EditText>(Resource.Id.CategoryEditText);
				RatingBar.NumStars = 10;
				BtnApply = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);
				ResetButton = view.FindViewById<TextView>(Resource.Id.Resetbutton);

				Methods.SetColorEditText(TxtSearchTerm, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtRelease, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtCountry, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtCategory, AppTools.IsTabDark() ? Color.White : Color.Black);

				Methods.SetFocusable(TxtRelease);
				Methods.SetFocusable(TxtCountry);
				Methods.SetFocusable(TxtCategory);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Event

		//Save data 
		private void BtnApplyOnClick(object sender, EventArgs e)
		{
			try
			{
				UserDetails.MoviesSearchTerm = TxtSearchTerm.Text;
				UserDetails.MoviesRating = Rating;
				UserDetails.MoviesRelease = TxtRelease.Text;
				UserDetails.MoviesCountry = CountryId;
				UserDetails.MoviesCategory = CategoryId;

				ContextMovies.MAdapter.MoviesList.Clear();
				ContextMovies.MAdapter.NotifyDataSetChanged();

				ContextMovies.SwipeRefreshLayout.Refreshing = true;
				ContextMovies.SwipeRefreshLayout.Enabled = true;

				ContextMovies.MainScrollEvent.IsLoading = false;
				ContextMovies.CountOffset = 1;

				ContextMovies.MRecycler.Visibility = ViewStates.Visible;
				ContextMovies.EmptyStateLayout.Visibility = ViewStates.Gone;

				Task.Factory.StartNew(() => ContextMovies.StartApiService());

				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ResetButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				TxtSearchTerm.Text = "";
				RatingBar.NumStars = 10;
				TxtRelease.Text = "";
				TxtCountry.Text = "";
				TxtCategory.Text = "";
				CountryId = "";
				CategoryId = "";

				UserDetails.MoviesSearchTerm = TxtSearchTerm.Text;
				UserDetails.MoviesRating = Rating;
				UserDetails.MoviesRelease = TxtRelease.Text;
				UserDetails.MoviesCountry = CountryId;
				UserDetails.MoviesCategory = TxtCategory.Text;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtReleaseOnTouch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;

				var dialogList = new MaterialAlertDialogBuilder(Context);

				TypeDialog = "Release";
				var arrayAdapter = ExperienceDate.ToList();

				dialogList.SetTitle(GetText(Resource.String.Lbl_Release));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtCountryOnTouch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;

				TypeDialog = "Country";

				var countriesArray = AppTools.GetCountryList(Activity);

				var dialogList = new MaterialAlertDialogBuilder(Context);

				var arrayAdapter = countriesArray.Select(item => item.Value).ToList();

				dialogList.SetTitle(GetText(Resource.String.Lbl_Country));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtCategoryOnTouch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;

				if (CategoriesController.ListCategoriesMovies.Count > 0)
				{
					TypeDialog = "Category";

					var dialogList = new MaterialAlertDialogBuilder(Context);

					var arrayAdapter = CategoriesController.ListCategoriesMovies.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();

					dialogList.SetTitle(GetText(Resource.String.Lbl_Category));
					dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
					dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

					dialogList.Show();
				}
				else
				{
					Methods.DisplayReportResult(Activity, "Not have List Categories Movies");
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		#endregion

		#region MaterialDialog

		public void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				string text = itemString;
				if (TypeDialog == "Category")
				{
					CategoryId = CategoriesController.ListCategoriesMovies.FirstOrDefault(categories => categories.Name == itemString)?.Id;
					TxtCategory.Text = text;
				}
				else if (TypeDialog == "Country")
				{
					var countriesArray = AppTools.GetCountryList(Activity);
					var check = countriesArray.FirstOrDefault(a => a.Value == itemString).Key;
					if (check != null)
					{
						CountryId = check;
					}

					TxtCountry.Text = itemString;
				}
				else if (TypeDialog == "Release")
				{
					TxtRelease.Text = itemString;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

	}
}