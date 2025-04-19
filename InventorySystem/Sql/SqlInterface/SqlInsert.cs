
using Sql.SqlDataTypes;

namespace Sql.SqlInterface;

public partial class SqlController
{

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
        if (!IsValidString(sqlData.Name)) return 0;
        if (!IsValidString(sqlData.Type)) return 0;
        if (!IsValidString(sqlData.Description)) return 0;
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
}