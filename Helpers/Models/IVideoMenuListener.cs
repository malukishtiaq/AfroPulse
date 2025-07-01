using PlayTube.PlayTubeClient.Classes.Global;

namespace PlayTube.Helpers.Models
{
	public interface IVideoMenuListener
	{
		public void RemoveVideo(VideoDataObject data);
	}
}