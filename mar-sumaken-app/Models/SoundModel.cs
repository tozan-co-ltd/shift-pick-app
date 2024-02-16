using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Models
{
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
                        DisplayName = "Error" + (i + 1).ToString(),
                        Item = soundErrorList[i].ToString()
                    };
                    soundList.Add(sound1);
                }

                return soundList;
            }
            set { }
        }

        public string DisplayName { get; set; }

        public string Item { get; set; }
    }
}


