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
        /// <param name="scanAndSound">ScanAndSound</param>
        public static async Task<string> GetPathOkSoundAsync(LocalDbService _dbService, ScanAndSound scanAndSound)
        {
            var scanAndSoundList = await _dbService.GetScanAndSound();
            scanAndSound = scanAndSoundList.Last();
            string okSoundNumber = new string(scanAndSound.OkSound.Where(char.IsDigit).ToArray());

            // get path of sound
            string resultOkSound = "ScanOkeySound/ScanOkeySound" + okSoundNumber + ".mp3";

            return resultOkSound;
        }

        /// <summary>
        /// エラーサウンドパスを取得
        /// </summary>
        /// <param name="_dbService">LocalDbService</param>
        /// <param name="scanAndSound">ScanAndSound</param>
        public static async Task<string> GetPathErrorSoundAsync(LocalDbService _dbService, ScanAndSound scanAndSound)
        {
            var scanAndSoundList = await _dbService.GetScanAndSound();
            scanAndSound = scanAndSoundList.Last();
            string errorSoundNumber = new string(scanAndSound.ErrorSound.Where(char.IsDigit).ToArray());

            // get path of sound
            string resultErrorSound = "ScanErrorSound/ScanErrorSound" + errorSoundNumber + ".mp3";

            return resultErrorSound;
        }
    }
}
