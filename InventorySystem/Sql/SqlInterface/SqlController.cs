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
        private SqlInMemory _warehouses;
        private SqlInMemory _items;
        public SqlController()
        {
            _warehouses = new SqlInMemory();
            _items = new SqlInMemory();
        }
        public async Task InitDataBaseConection()
        {
            await SqlCreateTable.InitialDataBaseTables();

            Task warehouseTask = _warehouses.Init("Warehouse");
            Task itemsTask = _items.Init("Item");

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
                await _warehouses.Init("Warehouse");
            }
            if (sqlData is SqlInventoryItem)
            {
                await _items.Init("Item");
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
            if (!_warehouses.Contains(sqlData.Location))
            {
                //not allowed
                return -1;
            }
            if (!_items.Contains(sqlData.Name))
            {
                //not allowed
                return -2;
            }

            return await AddRecord(sqlData);
        }


        #endregion

        #region DataBase Interactions
        public async Task<int> AddRecord(ISqlDataType sqlData)
        {
            string sqlCommand = $"INSERT INTO {sqlData.SqlTable} VALUES({sqlData.ToSql()})";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : 0;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        public async Task<int> RemoveRecord(ISqlDataType sqlData, string condition)
        {
            string sqlCommand = $"DELETE FROM {sqlData.SqlTable} WHERE {condition}";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : 0;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        public async Task<int> UpdateRecord(ISqlDataType sqlData, string setClause, string condition)
        {
            string sqlCommand = $"UPDATE {sqlData.SqlTable} SET {setClause} WHERE {condition}";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : 0;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        public async Task<List<ISqlDataType>> GetSortedRecords(ISqlDataType sqlData, string orderBy)
        {
            string sqlCommand = $"SELECT * FROM {sqlData.SqlTable} ORDER BY {orderBy}";
            var reader = await SqlAdapter.Instance.SqlQueryResult(sqlCommand);
            var records = new List<ISqlDataType>();

            if (reader != null)
            {
                while (await reader.ReadAsync())
                {
                    ISqlDataType record = ConvertReaderToSqlDataType(sqlData.SqlTable, reader);
                    records.Add(record);
                }
            }
            return records;
        }

        public async Task<List<ISqlDataType>> GetSortedSubList(ISqlDataType sqlData, string likeClause, string orderBy)
        {
            string sqlCommand = $"SELECT * FROM {sqlData.SqlTable} WHERE {likeClause} ORDER BY {orderBy}";
            var reader = await SqlAdapter.Instance.SqlQueryResult(sqlCommand);
            var records = new List<ISqlDataType>();

            if (reader != null)
            {
                while (await reader.ReadAsync())
                {
                    ISqlDataType record = ConvertReaderToSqlDataType(sqlData.SqlTable, reader);
                    records.Add(record);
                }
            }
            return records;
        }
        #endregion
        private ISqlDataType ConvertReaderToSqlDataType(string tableName, SqliteDataReader reader)
        {
            var dataType = CreateSqlDataTypeInstance(tableName);
            return dataType.FromSql(reader);
        }
        private ISqlDataType CreateSqlDataTypeInstance(string tableName)
        {
            return tableName switch
            {
                "Item" => new SqlInventoryItem(),
                "Inventory_Records" => new SqlInventoryRecord(),
                "Warehouse" => new SqlWarehouse(),
                _ => throw new ArgumentException("Unknown table name")
            };
        }
    }
}