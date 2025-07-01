using Android.Content;
using PlayTube.Activities.Live.Page;

namespace PlayTube.Activities.Live.Utils
{
	public class PrefManager
	{
		public static ISharedPreferences GetPreferences(Context context)
		{
			return context.GetSharedPreferences(LiveConstants.PrefName, FileCreationMode.Private);
		}
	}
}