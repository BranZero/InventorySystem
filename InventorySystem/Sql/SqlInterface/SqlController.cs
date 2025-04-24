using Sql.SqlDataTypes;
using System.Threading.Tasks;

namespace Sql.SqlInterface
{
    public partial class SqlController
    {
        //in memory list of all warehouse and items
        private SqlInMemory<SqlWarehouse> _warehouses;
        private SqlInMemory<SqlInventoryItem> _items;

        private SqlAdapter _sqlAdapter;

        public SqlController(string sqlDBPath)
        {
            _sqlAdapter = new SqlAdapter(sqlDBPath);
            _warehouses = new SqlInMemory<SqlWarehouse>(this);
            _items = new SqlInMemory<SqlInventoryItem>(this);
        }
        public async Task InitDataBaseConection()
        {
            await SqlCreateTable.InitialDataBaseTables(_sqlAdapter);

            Task warehouseTask = Task.Run(() => _warehouses.Init());
            Task itemsTask = Task.Run(() => _items.Init());

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
                await Task.Run(() => _warehouses.Init()); //intended to run in background
            }
            if (sqlData is SqlInventoryItem)
            {
                await Task.Run(() => _items.Init()); //intended to run in background
            }
        }

        #region DataBase Interactions
        private async Task<int> AddRecord<T>(T sqlData) where T : ISqlDataType
        {
            string sqlCommand = $"INSERT INTO {T.SqlTable} ({T.SqlColomns}) VALUES ({sqlData.ToSql()});";
            string result = await _sqlAdapter.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : -1;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        private async Task<int> RemoveRecord<T>(ISqlDataType sqlData, string condition) where T : ISqlDataType
        {
            string sqlCommand = $"DELETE FROM {T.SqlTable} WHERE {condition};";
            string result = await _sqlAdapter.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : -1;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        private async Task<int> UpdateRecord<T>(T sqlData, string setClause, string condition) where T : ISqlDataType
        {
            string sqlCommand = $"UPDATE {T.SqlTable} SET {setClause} WHERE {condition};";
            string result = await _sqlAdapter.SqlNoQueryResults(sqlCommand);
            int rowsAffected = int.TryParse(result, out int rows) ? rows : -1;

            await UpdateData(sqlData, rowsAffected);
            return rowsAffected;
        }

        internal async Task<List<T>?> GetSortedRecords<T>(string orderBy) where T : ISqlDataType
        {
            string sqlCommand = $"SELECT * FROM {T.SqlTable} ORDER BY {orderBy};";
            var records = await _sqlAdapter.SqlQueryResult<T>(sqlCommand); //TODO fix causes random errors
            return records;
        }

        internal async Task<List<T>?> GetSortedSubList<T>(string likeClause, string orderBy) where T : ISqlDataType
        {
            string sqlCommand = $"SELECT * FROM {T.SqlTable} WHERE {likeClause} ORDER BY {orderBy};";
            var records = await _sqlAdapter.SqlQueryResult<T>(sqlCommand);
            return records;
        }
        #endregion
        #region Validations
        public bool IsValidString(string line)
        {
            if(line == null)
            {
                return false;
            }
            return line.All(c => char.IsLetterOrDigit(c) || c == ' ');
        }

        public SqlAdapter GetAdpter()
        {
            return _sqlAdapter;
        }

        #endregion
    }
}
