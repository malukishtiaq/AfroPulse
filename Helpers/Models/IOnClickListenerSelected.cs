using Android.Views;
using PlayTube.PlayTubeClient.Classes.Messages;

namespace PlayTube.Helpers.Models
{
	public interface IOnClickListenerSelected
	{
		void OnItemClick(View view, GetChatsObject.Data obj, int pos);

		void OnItemLongClick(View view, GetChatsObject.Data obj, int pos);
	}
}