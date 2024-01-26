using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Common
{
    public class SoundPlayer
    {
        /// <summary>
        /// サウンドを実装
        /// </summary>
        /// <param name="sound">string</param>
        public static async void PlaySound(string sound) {
            var audioPlayer = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(sound));

            audioPlayer.Play();
        }
    }
}
