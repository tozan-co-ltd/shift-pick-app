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
                // ログインデータテーブル
                _connection.CreateTableAsync<LoginModel.LoginUserSqlLite>().Wait();
                // メニューデータテーブル
                _connection.CreateTableAsync<M_HandyMenuModel>().Wait();
                // 仕入先かんばんデータテーブル
                _connection.CreateTableAsync<M_SupplierKanbanModel>().Wait();
                // ハンディメニューデータテーブル
                _connection.CreateTableAsync<R_HandyMenuSupplierKanbanModel>().Wait();
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

        #region 仕入先かんばんテーブル操作
        /// <summary>
        /// 仕入先かんばん一覧を取得
        /// </summary>
        public Task<List<M_SupplierKanbanModel>> GetMSupplierKanbanAsync()
        {
            return _connection.Table<M_SupplierKanbanModel>().OrderBy(x => x.SupplierKanbanID).ToListAsync();
        }

        /// <summary>
        /// 仕入先かんばんを保存
        /// </summary>
        async public Task<int> SaveMSupplierKanbanAsync(M_SupplierKanbanModel m_SupplierKanbanModel)
        {
            return await _connection.InsertAsync(m_SupplierKanbanModel);
        }

        /// <summary>
        /// 古い仕入先かんばんデータを消す
        /// </summary>
        public async Task<int> ALLDeleteMSupplierKanbanAsync()
        {
            return await _connection.DeleteAllAsync<M_SupplierKanbanModel>();
        }

        /// <summary>
        /// 古いハンディメニューの仕入先かんばんデータドを取得
        /// </summary>
        public async Task<List<M_SupplierKanbanModel>> GetMSupplierKanbanByHandySupplierKanbanID(int supplierKanbanID)
        {
            return await _connection.Table<M_SupplierKanbanModel>().Where(x => x.SupplierKanbanID == supplierKanbanID).ToListAsync();
        }
        #endregion


        #region  ハンディメニューの仕入先かんばんテーブル操作
        /// <summary>
        /// ハンディメニューの仕入先かんばん一覧を取得
        /// </summary>
        public Task<List<R_HandyMenuSupplierKanbanModel>> GetRHandyMenuSupplierKanbanAsync()
        {
            return _connection.Table<R_HandyMenuSupplierKanbanModel>().OrderBy(x => x.HandyMenuID)
                            .ToListAsync();
        }

        /// <summary>
        /// ハンディメニューの仕入先かんばんを保存
        /// </summary>
        async public Task<int> SaveRHandyMenuSupplierKanbanAsync(R_HandyMenuSupplierKanbanModel rHandyMenuSupplierKanbanModel)
        {
            return await _connection.InsertAsync(rHandyMenuSupplierKanbanModel);
        }

        /// <summary>
        /// 古いハンディメニューの仕入先かんばんデータを消す
        /// </summary>
        public async Task<int> ALLDeleteRHandyMenuSupplierKanbanAsync()
        {
            return await _connection.DeleteAllAsync<R_HandyMenuSupplierKanbanModel>();
        }

        /// <summary>
        /// 古いハンディメニューの仕入先かんばんデータドを取得
        /// </summary>
        public async Task<List<R_HandyMenuSupplierKanbanModel>> GetRHandyMenuSupplierKanbanByHandyMenuID(int handyMenuID)
        {
            return await _connection.Table<R_HandyMenuSupplierKanbanModel>().Where(x => x.HandyMenuID == handyMenuID).ToListAsync();
        }
        #endregion

    }
}