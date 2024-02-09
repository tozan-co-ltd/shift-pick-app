using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Interface
{
    public interface IAssemblyService
    {
        string GetPackageName();
        string GetVersionName();
        string GetVersionCode();
    }
}
