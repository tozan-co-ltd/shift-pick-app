using shift_pick_app.Models;
using SQLite;

namespace shift_pick_app.Services
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
                // ログインデータテーブル
                _connection.CreateTableAsync<LoginModel.LoginUserSqlLite>().Wait();
                // メニューデータテーブル
                _connection.CreateTableAsync<M_HandyMenuModel>().Wait();
                // 設定データテーブル
                _connection.CreateTableAsync<HandySettingModel.HandySetting>().Wait();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region ハンディ設定テーブル操作
        /// <summary>
        /// スキャンとサウンドを取得
        /// </summary>
        public async Task<List<HandySettingModel.HandySetting>> GetSetting()
        {
            return await _connection.Table<HandySettingModel.HandySetting>().ToListAsync();
        }

        /// <summary>
        /// スキャンとサウンドを取得
        /// </summary>
        public async Task<HandySettingModel.HandySetting> GetByCompanyID(int Id)
        {
            return await _connection.Table<HandySettingModel.HandySetting>().Where(x => x.Id == Id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 作成
        /// </summary>
        public async Task Create(HandySettingModel.HandySetting handySetting)
        {
            await _connection.InsertAsync(handySetting);
        }

        /// <summary>
        /// 更新
        /// </summary>
        public async Task Update(HandySettingModel.HandySetting handySetting)
        {
            await _connection.UpdateAsync(handySetting);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public async Task Delete(HandySettingModel.HandySetting handySetting)
        {
            await _connection.DeleteAsync(handySetting);
        }

        /// <summary>
        /// 古いハンディ設定ーデータを消す
        /// </summary>
        public async Task<int> ALLDeleteHandySettingAsync()
        {
            return await _connection.DeleteAllAsync<HandySettingModel.HandySetting>();
        }
        #endregion



        #region ログインテーブル操作
        /// <summary>
        /// 作成
        /// </summary>
        public async Task Create(LoginModel.LoginUserSqlLite loginUserSqlLite)
        {
            await _connection.InsertAsync(loginUserSqlLite);
        }

        /// <summary>
        /// ログインの非同期を取得する
        /// </summary>
        public async Task<List<LoginModel.LoginUserSqlLite>> GetLoginAsync()
        {
            return await _connection.Table<LoginModel.LoginUserSqlLite>().ToListAsync();
        }

        /// <summary>
        /// ログインユーザーを保存
        /// </summary>
        public async Task<int> SaveLoginAsync(LoginModel.LoginUserSqlLite login)
        {
            await _connection.DeleteAllAsync<LoginModel.LoginUserSqlLite>();
            return await _connection.InsertAsync(login);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public async Task DeleteUserLogin(LoginModel.LoginUserSqlLite loginUserSqlLite)
        {
            await _connection.DeleteAsync(loginUserSqlLite);
        }
        #endregion


        #region メニューテーブル操作
        /// <summary>
        /// メニュー一覧を取得
        /// </summary>
        public Task<List<M_HandyMenuModel>> GetMenuAsync()
        {
            return _connection.Table<M_HandyMenuModel>().OrderBy(x => x.SortNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// メニューを保存
        /// </summary>
        async public Task<int> SaveMenuAsync(M_HandyMenuModel menu)
        {
            return await _connection.InsertAsync(menu);
        }

        /// <summary>
        /// 古いメニューデータを消す
        /// </summary>
        public async Task<int> ALLDeleteMenuAsync()
        {
            return await _connection.DeleteAllAsync<M_HandyMenuModel>();
        }
        #endregion

    }
}