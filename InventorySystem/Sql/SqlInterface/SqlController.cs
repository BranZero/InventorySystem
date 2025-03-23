using Microsoft.Data.Sqlite;
using Sql.SqlDataTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.SqlInterface
{
    public class SqlController
    {
        public SqlController()
        {
            SqlCreateTable.InitialDataBaseTables();
        }

        #region Requests
        public async Task<int> AddRecord(ISqlDataType sqlData)
        {
            string sqlCommand = $"INSERT INTO {sqlData.SqlTable} VALUES({sqlData.ToSql()})";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            return int.TryParse(result, out int rowsAffected) ? rowsAffected : 0;
        }

        public async Task<int> RemoveRecord(string tableName, string condition)
        {
            string sqlCommand = $"DELETE FROM {tableName} WHERE {condition}";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            return int.TryParse(result, out int rowsAffected) ? rowsAffected : 0;
        }

        public async Task<int> UpdateRecord(string tableName, string setClause, string condition)
        {
            string sqlCommand = $"UPDATE {tableName} SET {setClause} WHERE {condition}";
            string result = await SqlAdapter.Instance.SqlNoQueryResults(sqlCommand);
            return int.TryParse(result, out int rowsAffected) ? rowsAffected : 0;
        }

        public async Task<List<ISqlDataType>> GetSortedRecords(string tableName, string orderBy)
        {
            string sqlCommand = $"SELECT * FROM {tableName} ORDER BY {orderBy}";
            var reader = await SqlAdapter.Instance.SqlQueryResult(sqlCommand);
            var records = new List<ISqlDataType>();
            if (reader != null)
            {
                while (await reader.ReadAsync())
                {
                    ISqlDataType record = ConvertReaderToSqlDataType(tableName, reader);
                    records.Add(record);
                }
            }
            return records;
        }

        public async Task<List<ISqlDataType>> GetSortedSubList(string tableName, string likeClause, string orderBy)
        {
            string sqlCommand = $"SELECT * FROM {tableName} WHERE {likeClause} ORDER BY {orderBy}";
            var reader = await SqlAdapter.Instance.SqlQueryResult(sqlCommand);
            var records = new List<ISqlDataType>();
            if (reader != null)
            {
                while (await reader.ReadAsync())
                {
                    ISqlDataType record = ConvertReaderToSqlDataType(tableName, reader);
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