using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Common
{
    public static class Const
    {
        // メニュー
        public const int C_NUMBER_OF_MENU = 10;
        public const string C_REGISTER_STORE_IN_PAGE = "入庫";
        public const string C_REGISTER_BULK_STORE_IN_PAGE = "まとめ入庫";
        public const string C_SEARCH_SHIPPING_INSTRUCTION_IN_PAGE = "出庫";

        // サウンド
        public const string C_SOUND_ERROR_FOLDER = "ScanErrorSound/ScanErrorSound";
        public const string C_SOUND_OK_FOLDER = "ScanOkeySound/ScanOkeySound";

        // 権限
        public const int C_AUTHORIZED_KUBUN_WORKER = 3; // 作業者

    }
}
