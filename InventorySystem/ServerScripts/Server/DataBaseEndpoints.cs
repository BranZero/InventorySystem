
using Microsoft.AspNetCore.Mvc;
using Sql.SqlInterface;
using Sql.SqlDataTypes;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace InventorySystem.Controllers
{
    public partial class HomeController : Controller
    {
        #region Sql DataBase Get

        [HttpGet("api/inventory")]
        public async Task<IActionResult> GetInventory([FromQuery] string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return BadRequest("Location query parameter is required.");
            }

            List<SqlInventoryRecord>? inventory = await _sqlController.GetSortedRecords<SqlInventoryRecord>("name");
            if (inventory == null || inventory.Capacity == 0)
            {
                return NotFound($"No inventory found for location: {location}");
            }

            string json = JsonSerializer.Serialize(inventory, SqlInventoryRecordSerializationOptions.Default.SqlInventoryRecord);
            return Content(json, "application/json");
        }

        [HttpGet("api/search")]
        public async Task<IActionResult> SearchInventory([FromQuery] string search, [FromQuery] string type)
        {
            if (string.IsNullOrEmpty(search) || !StringValidation(search))
            {
                return BadRequest("Search query parameter is required.");
            }
            if (string.IsNullOrEmpty(type) || !StringValidation(type))
            {
                return BadRequest("Type query parameter is required.");
            }

            string json;
            if (type == "Item")
            {
                List<SqlInventoryItem>? inventory = await _sqlController.GetSortedSubList<SqlInventoryItem>(search, "name");
                if (inventory == null || inventory.Capacity == 0)
                {
                    return NotFound($"No inventory found for location: {search}");
                }

                json = JsonSerializer.Serialize(inventory, SqlInventoryRecordSerializationOptions.Default.SqlInventoryRecord);
            }
            else if (type == "Warehouse")
            {
                List<SqlWarehouse>? inventory = await _sqlController.GetSortedSubList<SqlWarehouse>(search, "name");
                if (inventory == null || inventory.Capacity == 0)
                {
                    return NotFound($"No inventory found for location: {search}");
                }

                json = JsonSerializer.Serialize(inventory, SqlInventoryRecordSerializationOptions.Default.SqlInventoryRecord);
            }
            else
            {
                return BadRequest("Invalid type query parameter.");
            }

            return Content(json, "application/json");
        }


        #endregion
        #region Sql DataBase Post

        [HttpPost("api/add-inventory-record")]
        public async Task<IActionResult> AddInventoryRecord([FromBody] SqlInventoryRecord record)
        {
            if (record.Item == null)
            {
                return BadRequest("Invalid JSON data.");
            }

            SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(record);
            switch (result)
            {
                case SqlInventoryRecordResult.Success:
                    return StatusCode(StatusCodes.Status201Created, "Record was successfully added.");
                case SqlInventoryRecordResult.ManyChanges:
                    //Logger.Instance.Log(LogLevel.Error, $"An insert resulted in many changes to the database from the following: {record}");
                    return Ok("Error: Failed to add record");
                case SqlInventoryRecordResult.NothingHappend:
                    //Logger.Instance.Log(LogLevel.Error, $"An insert resulted in no changes to the database from the following: {record}");
                    return Ok("Error: Failed to add record");
                default:
                    return Ok("Error: Failed to add record");
            }
        }

        [HttpPost("api/add-item")]
        public async Task<IActionResult> AddItem([FromBody] SqlInventoryItem record)
        {
            if (record.Name == null)
            {
                return BadRequest("Invalid JSON data.");
            }

            int amount = await _sqlController.InsertItem(record);
            if (amount == 1)
            {
                return StatusCode(StatusCodes.Status201Created, "Item added successfully!");
            }
            else
            {
                return Ok("Item failed to add!");
            }
        }

        [HttpPost("api/add-warehouse")]
        public async Task<IActionResult> AddWarehouse([FromBody] SqlWarehouse record)
        {
            if (record.Name == null)
            {
                return BadRequest("Invalid JSON data.");
            }

            int amount = await _sqlController.InsertWarehouse(record);
            if (amount == 1)
            {
                return StatusCode(StatusCodes.Status201Created, "Warehouse added successfully!");
            }
            else
            {
                return Ok("Warehouse failed to add!");
            }
        }

        #endregion
        #region Sql DataBase Updates

        #endregion
        #region Sql DataBase Deletions

        #endregion
    }
}