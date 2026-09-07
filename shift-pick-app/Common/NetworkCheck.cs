using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace shift_pick_app.Common
{
    public class NetworkCheck
    {
        /// <summary>
        /// ネットワーク接続チェック
        /// </summary>
        public static bool CheckNetworkConnection()
        {

            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            if (accessType != NetworkAccess.Internet)
                return false;

            return true;
        }
    }
}
