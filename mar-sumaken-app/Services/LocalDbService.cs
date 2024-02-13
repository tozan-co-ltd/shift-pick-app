using mar_sumaken_app.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mar_sumaken_app.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "local_db.db3";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
            try
            {
                //ログインデータテーブル
                _connection.CreateTableAsync<Login.LoginUserSqlLite>().Wait();
                //設定データテーブル
                _connection.CreateTableAsync<Setting.HandySetting>().Wait();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region Settingテーブル操作
        /// <summary>
        /// スキャンとサウンドを取得
        /// </summary>
        public async Task<List<Setting.HandySetting>> GetSetting()
        {
            return await _connection.Table<Setting.HandySetting>().ToListAsync();
        }

        /// <summary>
        /// スキャンとサウンドを取得
        /// </summary>
        public async Task<Setting.HandySetting> GetByCompanyID(int Id)
        {
            return await _connection.Table<Setting.HandySetting>().Where(x => x.Id == Id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 作成
        /// </summary>
        public async Task Create(Setting.HandySetting handySetting)
        {
            await _connection.InsertAsync(handySetting);
        }

        /// <summary>
        /// 更新
        /// </summary>
        public async Task Update(Setting.HandySetting handySetting)
        {
            await _connection.UpdateAsync(handySetting);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public async Task Delete(Setting.HandySetting handySetting)
        {
            await _connection.DeleteAsync(handySetting);
        }
        #endregion



        #region Loginテーブル操作
        /// <summary>
        /// 作成
        /// </summary>
        public async Task Create(Login.LoginUserSqlLite loginUserSqlLite)
        {
            await _connection.InsertAsync(loginUserSqlLite);
        }

        /// <summary>
        /// ログインの非同期を取得する
        /// </summary>
        public async Task<List<Login.LoginUserSqlLite>> GetLoginAsync()
        {
            return await _connection.Table<Login.LoginUserSqlLite>().ToListAsync();
        }

        /// <summary>
        /// ログインユーザーを保存
        /// </summary>
        public async Task<int> SavLoginAsync(Login.LoginUserSqlLite login)
        {
            await _connection.DeleteAllAsync<Login.LoginUserSqlLite>();
            return await _connection.InsertAsync(login);
        }
        #endregion
    }
}