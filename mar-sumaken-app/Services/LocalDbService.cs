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
            _connection.CreateTableAsync<ScanAndSound>();
        }

        /// <summary>
        /// スキャンとサウンドを取得
        /// </summary>
        public async Task<List<ScanAndSound>> GetScanAndSound()
        {
            return await _connection.Table<ScanAndSound>().ToListAsync();
        }

        /// <summary>
        /// スキャンとサウンドを取得
        /// </summary>
        public async Task<ScanAndSound> GetById(int id)
        {
            return await _connection.Table<ScanAndSound>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 作成
        /// </summary>
        public async Task Create(ScanAndSound scanAndSound)
        {
            await _connection.InsertAsync(scanAndSound);
        }

        /// <summary>
        /// 更新
        /// </summary>
        public async Task Update(ScanAndSound scanAndSound)
        {
            await _connection.UpdateAsync(scanAndSound);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public async Task Delete(ScanAndSound scanAndSound)
        {
            await _connection.DeleteAsync(scanAndSound);
        }
    }
}