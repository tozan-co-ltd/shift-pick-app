using shift_pick_app.Models;
using shift_pick_app.Services;

namespace shift_pick_app.Common
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
            // 設定がない場合は1番目のサウンド
            string okSoundNumber = "1";
            if (scanAndSoundList.Count > 0)
            {
                handySetting = scanAndSoundList.Last();
                okSoundNumber = new string(handySetting.ScanOkSound.Where(char.IsDigit).ToArray());
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
            // 設定がない場合は1番目のサウンド
            string errorSoundNumber = "1";
            if (scanAndSoundList.Count > 0)
            {
                handySetting = scanAndSoundList.Last();
                errorSoundNumber = new string(handySetting.ScanErrorSound.Where(char.IsDigit).ToArray());
            }

            // get path of sound
            string resultErrorSound = "ScanErrorSound/ScanErrorSound" + errorSoundNumber + ".mp3";

            return resultErrorSound;
        }
    }
}
