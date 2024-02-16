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
                // メニューデータテーブル
                _connection.CreateTableAsync<HandyMenu>().Wait();
                //設定データテーブル
                _connection.CreateTableAsync<Setting.HandySetting>().Wait();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region 設定テーブル操作
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



        #region ログインテーブル操作
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
        public async Task<int> SaveLoginAsync(Login.LoginUserSqlLite login)
        {
            await _connection.DeleteAllAsync<Login.LoginUserSqlLite>();
            return await _connection.InsertAsync(login);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public async Task DeleteUserLogin(Login.LoginUserSqlLite loginUserSqlLite)
        {
            await _connection.DeleteAsync(loginUserSqlLite);
        }
        #endregion


        #region メニューテーブル操作
        /// <summary>
        /// メニュー一覧を取得
        /// </summary>
        public Task<List<HandyMenu>> GetMenuAsync()
        {
            return _connection.Table<HandyMenu>().OrderBy(x => x.SortNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// メニューを保存
        /// </summary>
        async public Task<int> SavMenuAsync(HandyMenu menu0)
        {
            return await _connection.InsertAsync(menu0);
        }

        /// <summary>
        /// 古いメニューデータを消す
        /// </summary>
        public async Task<int> ALLDeleteMenuAsync()
        {
            return await _connection.DeleteAllAsync<HandyMenu>();
        }
        #endregion
    }
}