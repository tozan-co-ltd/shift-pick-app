using System.Reflection;

namespace shift_pick_app.Models
{
    /// <summary>
    /// TODO: 不要になる可能性がある場合は、このクラスの削除を検討する。
    /// または必要な場合は、すべてのサウンドファイルについて "Scan" 接頭辞を取り除くよう、名前の変更を検討する。
    /// </summary>
    public class SoundModel
    {
        /// <summary>
        /// サウンドOKリストを取得
        /// </summary>
        public List<SoundModel> SoundOkeyList
        {
            get
            {
                var soundList = new List<SoundModel>();

                var executingAssembly = Assembly.GetExecutingAssembly();
                string folderName = string.Format("{0}.Resources.Raw.ScanOkeySound", executingAssembly.GetName().Name.Replace("-", "_"));
                var soundOkeyList = executingAssembly.GetManifestResourceNames().Where(r => r.StartsWith(folderName) && r.EndsWith(".mp3")).ToList();

                for (int i = 0; i < soundOkeyList.Count(); ++i)
                {
                    SoundModel sound1 = new SoundModel
                    {
                        Number = i+1,
                        DisplayName = "OK" + (i + 1).ToString(),
                        Item = soundOkeyList[i].ToString()
                    };
                    soundList.Add(sound1);
                }

                return soundList;
            }
            set { }
        }

        /// <summary>
        /// サウンドエラーリストを取得
        /// </summary>
        public List<SoundModel> SoundErrorList
        {
            get
            {
                var soundList = new List<SoundModel>();

                var executingAssembly = Assembly.GetExecutingAssembly();
                string folderName = string.Format("{0}.Resources.Raw.ScanErrorSound", executingAssembly.GetName().Name.Replace("-", "_"));
                var resourceNames = executingAssembly.GetManifestResourceNames();

                var soundErrorList = executingAssembly.GetManifestResourceNames().Where(r => r.StartsWith(folderName) && r.EndsWith(".mp3")).ToList();

                for (int i = 0; i < soundErrorList.Count(); ++i)
                {
                    SoundModel sound1 = new SoundModel
                    {
                        Number = i+1,
                        DisplayName = "Error" + (i + 1).ToString(),
                        Item = soundErrorList[i].ToString()
                    };
                    soundList.Add(sound1);
                }

                return soundList;
            }
            set { }
        }

        public int Number { get; set; }
        public string DisplayName { get; set; }
        public string Item { get; set; }
    }
}


