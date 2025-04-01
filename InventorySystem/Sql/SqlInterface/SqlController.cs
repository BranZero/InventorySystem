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
                _ = Task.Run(() => _warehouses.Init("Warehouse")); //intented to run in background
            }
            if (sqlData is SqlInventoryItem)
            {
                _ = Task.Run(() => _items.Init("Item")); //intented to run in background
            }
        }

        #region Insert Validations
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlData"></param>
        /// <returns>greater then 0 is success less then is an error </returns>
        public async Task<int> InsertInventoryRecord(SqlInventoryRecord sqlData)
        {
            //check if valid
            if (!_warehouses.Contains(sqlData.Location, out int id))
            {
                //Warehouse doesn't exist yet
                return -1;
            }
            if (!_items.Contains(sqlData.Item, out int id2))
            {
                //item doesn't exist in item list yet 
                return -2;
            }
            sqlData.Location = id.ToString();
            sqlData.Item = id2.ToString();
            return await AddRecord(sqlData);
        }

        public async Task<int> InsertItem(SqlInventoryItem sqlData){
            if (_items.Contains(sqlData.Name, out _))
            {
                //item does exist in item list yet 
                return 0;
            }
            return await AddRecord(sqlData);
        }

        public async Task<int> InsertWarehouse(SqlWarehouse sqlData){
            if (_warehouses.Contains(sqlData.Name, out _))
            {
                //warehouse does exist in warehouse list yet 
                return -1;
            }
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
    }
}