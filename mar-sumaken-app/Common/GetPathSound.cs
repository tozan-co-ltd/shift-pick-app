using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mar_sumaken_app.Models;
using mar_sumaken_app.Common;
using Plugin.Maui.Audio;
using mar_sumaken_app.Services;

namespace mar_sumaken_app.Common
{
    public class GetPathSound
    {
        /// <summary>
        /// OKサウンドパスを取得
        /// </summary>
        /// <param name="_dbService">LocalDbService</param>
        /// <param name="handySetting">Setting.HandySetting</param>
        public static async Task<string> GetPathOkSoundAsync(LocalDbService _dbService, Setting.HandySetting handySetting)
        {
            var scanAndSoundList = await _dbService.GetSetting();
            handySetting = scanAndSoundList.Last();
            string okSoundNumber = new string(handySetting.ScanOkSound.Where(char.IsDigit).ToArray());

            // get path of sound
            string resultOkSound = "ScanOkeySound/ScanOkeySound" + okSoundNumber + ".mp3";

            return resultOkSound;
        }

        /// <summary>
        /// エラーサウンドパスを取得
        /// </summary>
        /// <param name="_dbService">LocalDbService</param>
        /// <param name="handySetting">Setting.HandySetting</param>
        public static async Task<string> GetPathErrorSoundAsync(LocalDbService _dbService, Setting.HandySetting handySetting)
        {
            var scanAndSoundList = await _dbService.GetSetting();

            handySetting = scanAndSoundList.Last();
            string errorSoundNumber = new string(handySetting.ScanErrorSound.Where(char.IsDigit).ToArray());

            // get path of sound
            string resultErrorSound = "ScanErrorSound/ScanErrorSound" + errorSoundNumber + ".mp3";

            return resultErrorSound;
        }
    }
}
