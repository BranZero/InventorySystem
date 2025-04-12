using Microsoft.Data.Sqlite;
using Sql.SqlDataTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.SqlInterface
{
    public class SqlController
    {
        //in memory list of all warehouse and items
        private SqlInMemory<SqlWarehouse> _warehouses;
        private SqlInMemory<SqlInventoryItem> _items;
        public SqlController()
        {
            _warehouses = new SqlInMemory<SqlWarehouse>(this);
            _items = new SqlInMemory<SqlInventoryItem>(this);
        }
        public async Task InitDataBaseConection()
        {
            await SqlCreateTable.InitialDataBaseTables();

            Task warehouseTask = Task.Run(() => _warehouses.Init("Warehouse"));
            Task itemsTask = Task.Run(() => _items.Init("Item"));

            await warehouseTask;
            await itemsTask;
        }

        private async Task UpdateData(ISqlDataType sqlData, int rowsAffected)
        {
            if (rowsAffected == 0)
            {
                return;
            }
            if (sqlData is SqlWarehouse)
            {
                await Task.Run(() => _warehouses.Init("Warehouse")); //intended to run in background
            }
            if (sqlData is SqlInventoryItem)
            {
                await Task.Run(() => _items.Init("Item")); //intended to run in background
            }
        }

        #region Insert Validations
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlData"></param>
        /// <returns>greater then 0 is success less then is an error </returns>
        public async Task<SqlInventoryRecordResult> InsertInventoryRecord(SqlInventoryRecord sqlData)
        {
            //Warehouse doesn't exist yet
            if (!_warehouses.Contains(sqlData.Location)) return SqlInventoryRecordResult.InvalidWarehouse;
            //item doesn't exist in item list yet 
            if (!_items.Contains(sqlData.Item)) return SqlInventoryRecordResult.InvalidItem;

            if (!SqlInventoryRecord.IsValidRarity(sqlData.Rarity)) return SqlInventoryRecordResult.InvalidRarity;

            if (sqlData.Quantity <= 0) return SqlInventoryRecordResult.QuantityAtOrBelowZero;

            if (sqlData.Price < 0) return SqlInventoryRecordResult.PriceBelowZero;

            int result = await AddRecord(sqlData);

            //Should be only one row changed
            if (result == 1) return SqlInventoryRecordResult.Success;
            if (result == 0) return SqlInventoryRecordResult.NothingHappend;
            return SqlInventoryRecordResult.ManyChanges;
        }

        public async Task<int> InsertItem(SqlInventoryItem sqlData)
        {
            //item name exists in item list already
            if (_items.Contains(sqlData.Name)) return 0;
            return await AddRecord(sqlData);
        }

        public async Task<int> InsertWarehouse(SqlWarehouse sqlData)
        {
            if (!IsValidString(sqlData.Name)) return 0;
            //warehouse name exists in warehouse list already
            if (_warehouses.Contains(sqlData.Name)) return 0;
            return await AddRecord(sqlData);
        }

        #endregion

        #region DataBase Interactions
        private async Task<int> AddRecord<T>(T sqlData) where T : ISqlDataType
        {
            string sqlCommand = $"INSERT INTO {T.SqlTable} ({T.SqlColomns}) VALUES ({sqlData.ToSql()})";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : -1;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        private async Task<int> RemoveRecord<T>(ISqlDataType sqlData, string condition) where T : ISqlDataType
        {
            string sqlCommand = $"DELETE FROM {T.SqlTable} WHERE {condition}";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : -1;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        private async Task<int> UpdateRecord<T>(T sqlData, string setClause, string condition) where T : ISqlDataType
        {
            string sqlCommand = $"UPDATE {T.SqlTable} SET {setClause} WHERE {condition}";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : -1;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        internal async Task<List<T>?> GetSortedRecords<T>(string orderBy) where T : ISqlDataType
        {
            string sqlCommand = $"SELECT * FROM {T.SqlTable} ORDER BY {orderBy}";
            var records = await SqlAdapter.Instance.SqlQueryResult<T>(sqlCommand);
            return records;
        }

        public async Task<List<T>?> GetSortedSubList<T>(string likeClause, string orderBy) where T : ISqlDataType
        {
            string sqlCommand = $"SELECT * FROM {T.SqlTable} WHERE {likeClause} ORDER BY {orderBy}";
            var records = await SqlAdapter.Instance.SqlQueryResult<T>(sqlCommand);
            return records;
        }
        #endregion
        #region Validations
        public bool IsValidString(string line)
        {
            return line.All(c => char.IsLetterOrDigit(c) || c == ' ');
        }

        #endregion
    }
}
