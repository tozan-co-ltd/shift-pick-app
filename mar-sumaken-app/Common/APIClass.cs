using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Common
{
    public class APIClass
    {
        // タイムアウト時間の設定(10秒)
        private readonly int HttpClientTimeOut = 10000;

        public async Task<(bool success, string content)> GetMethod(string getApiUrl)
        {

#if DEBUG
            HttpClientHandler insecureHandler = GetInsecureHandler();
            HttpClient httpClient = new HttpClient(insecureHandler);
#else
    HttpClient httpClient = new HttpClient();
#endif
            // POST送信
            HttpResponseMessage response;

            try
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(HttpClientTimeOut);
                response = await httpClient.GetAsync(getApiUrl);
                string responseContent = response.Content.ReadAsStringAsync().Result;
                return (response.IsSuccessStatusCode, responseContent);
            }
            catch (Exception ex)
            {
                var w32ex = ex as Win32Exception;
                if(w32ex == null)
                {
                    w32ex = ex.InnerException as Win32Exception;
                    if (ex.Message != null)
                        return (false, "接続先URLにアクセスできません");
                }
                if (w32ex != null)
                {
                    int code = w32ex.ErrorCode;
                    if (code == 10060)
                        return (false, "接続先URLにアクセスできません");
                    else
                        throw;
                }

                throw;
            }
        }

        public async Task<(bool success, string content)> PostMethod(string jsonPostData, string apiUrl, string controllerName, int companyID = 0)
        {
            string postUrl = "";
            if (companyID == 0)
                postUrl = apiUrl + controllerName;
            else
                postUrl = Util.AddCompanyPath(apiUrl + controllerName, companyID);

#if DEBUG
            HttpClientHandler insecureHandler = GetInsecureHandler();
            HttpClient httpClient = new HttpClient(insecureHandler);
#else
    HttpClient httpClient = new HttpClient();
#endif

            var content = new StringContent(jsonPostData, Encoding.UTF8, @"application/json");

            // POST送信
            HttpResponseMessage response;
            try
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(HttpClientTimeOut);
                response = await httpClient.PostAsync(postUrl, content);
                string responseContent = response.Content.ReadAsStringAsync().Result;
                return (response.IsSuccessStatusCode, responseContent);
            }
            catch (Exception ex)
            {
                var w32ex = ex as Win32Exception;
                if (w32ex == null)
                {
                    w32ex = ex.InnerException as Win32Exception;
                    if(ex.Message != null)
                        return (false, "接続先URLにアクセスできません");
                }
                if (w32ex != null)
                {
                    int code = w32ex.ErrorCode;
                    if (code == 10060 || code == 10065)
                        return (false, "接続先URLにアクセスできません");
                    else
                        throw;
                }

                throw;
            }
        }

        // 安全でないハンドラーの取得
        public HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }
    }
}
