using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mar_sumaken_app.Models;
using mar_sumaken_app.Common;
using Plugin.Maui.Audio;
using mar_sumaken_app.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace mar_sumaken_app.Common
{
    /// <summary>
    /// ハッシュ化に関する関数
    /// </summary>
    /// <remarks>
    /// 参考:https://qiita.com/Nossa/items/0af4429ceb7628d46909
    /// </remarks>
    public static class Hashing
    {
        /// <summary>
        /// 16進数⽂字列をbyte列に変換
        /// </summary>
        /// <param name="stringSalt">16進数⽂字列のソルト</param>
        /// <returns>byte列のソルト</returns>
        public static byte[] ConvertStringToBytes(string stringSalt)
        {
            var listSalt = new List<byte>();
            for (int i = 0; i < stringSalt.Length / 2; i++)
            {
                listSalt.Add(Convert.ToByte(stringSalt.Substring(i * 2, 2), 16));
            }
            return listSalt.ToArray();
        }


        /// <summary>
        /// 平⽂パスワードをハッシュ化されたパスワードに変換
        /// </summary>
        /// <param name="plaintext">平文パスワード</param>
        /// <param name="byteSalt">byte列のソルト</param>
        /// <returns>16進数⽂字列のパスワード</returns>
        /// <remarks>
        /// 参考:https://docs.microsoft.com/ja-jp/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-3.0
        /// </remarks>
        public static string ConvertPlaintextPasswordToHashedPassword(string plaintext, byte[] byteSalt)
        {
            //Pbkdf2メソッドでハッシュ化する反復回数や作成するハッシュの長さを指定
            byte[] hash = KeyDerivation.Pbkdf2(
              plaintext,
              byteSalt,
              prf: KeyDerivationPrf.HMACSHA256,
              iterationCount: 10000,                //反復回数
              numBytesRequested: 256 / 8);          //ハッシュの長さ

            //ハッシュ値を16進数文字列に変換
            var hashedPassword = string.Concat(hash.Select(b => $"{b:x2}"));
            return hashedPassword;
        }

        /// <summary>
        /// byte列を16進数⽂字列に変換
        /// </summary>
        /// <param name="byteSalt">byte列のソルト</param>
        /// <returns>16進数⽂字列のソルト</returns>
        public static string ConvertByteToString(byte[] byteSalt)
        {
            return BitConverter.ToString(byteSalt).Replace("-", string.Empty);
        }
    }
}
