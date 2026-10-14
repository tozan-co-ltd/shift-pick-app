using Plugin.Maui.Audio;

namespace shift_pick_app.Common
{
    public class SoundPlayer
    {
        /// <summary>
        /// サウンドを実装
        /// </summary>
        /// <param name="sound">string</param>
        public static async void PlaySound(string sound) {
            var audioPlayer = AudioManager.Current.CreatePlayer(await Util.OpenSoundAsync(sound));

            audioPlayer.Play();
        }
    }
}
