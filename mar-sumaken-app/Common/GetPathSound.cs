using mar_sumaken_app.Models;
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
        public static async Task<string> GetPathOkSoundAsync(LocalDbService _dbService, HandySettingModel.HandySetting handySetting)
        {
            var scanAndSoundList = await _dbService.GetSetting();
            string okSoundNumber = string.Empty;
            if (scanAndSoundList.Count > 0)
            {
                handySetting = scanAndSoundList.Last();
                okSoundNumber = new string(handySetting.ScanOkSound.Where(char.IsDigit).ToArray());
            }
            else
            {
                SoundModel sound = new SoundModel();
                okSoundNumber = sound.SoundOkeyList.FirstOrDefault().Number.ToString();
            }

            // get path of sound
            string resultOkSound = "ScanOkeySound/ScanOkeySound" + okSoundNumber + ".mp3";

            return resultOkSound;
        }

        /// <summary>
        /// エラーサウンドパスを取得
        /// </summary>
        /// <param name="_dbService">LocalDbService</param>
        /// <param name="handySetting">Setting.HandySetting</param>
        public static async Task<string> GetPathErrorSoundAsync(LocalDbService _dbService, HandySettingModel.HandySetting handySetting)
        {
            var scanAndSoundList = await _dbService.GetSetting();
            string errorSoundNumber = string.Empty;
            if (scanAndSoundList.Count > 0)
            {
                handySetting = scanAndSoundList.Last();
                errorSoundNumber = new string(handySetting.ScanErrorSound.Where(char.IsDigit).ToArray());
            }
            else
            {
                SoundModel sound = new SoundModel();
                errorSoundNumber = sound.SoundErrorList.FirstOrDefault().Number.ToString();
            }

            // get path of sound
            string resultErrorSound = "ScanErrorSound/ScanErrorSound" + errorSoundNumber + ".mp3";

            return resultErrorSound;
        }
    }
}
