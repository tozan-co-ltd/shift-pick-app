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
                var createTableSetting = _connection.CreateTableAsync<Setting.HandySetting>();
                createTableSetting.Wait(); // Wait for table creation to finish
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// Setting.HandySetting

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
    }
}