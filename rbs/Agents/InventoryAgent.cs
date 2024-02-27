using System;
using System.Collections.Generic;

public class InventoryAgent
{
    public List<Manufactur> GetManufacturers()
    {
        var mysqlDataAgent = new MySqlDataAgent();
        return mysqlDataAgent.GetManufacturers();
    }

    public List<Inventory> GetInventories(string manufacturerId)
    {
        var mysqlDataAgent = new MySqlDataAgent();
        return mysqlDataAgent.GetInventories(manufacturerId);
    }

    public Inventory GetInventoryByProductNumber(string ProductNumber)
    {
        var mysqlDataAgent = new MySqlDataAgent();
        return mysqlDataAgent.GetInventoryByProductNumber(ProductNumber);
    }

    public bool SaveInventories(IEnumerable<Inventory> inventories)
    {
        var mysqlDataAgent = new MySqlDataAgent();
        return mysqlDataAgent.SaveInventories(inventories);
    }

    public List<Inventory> InventoryReport(DateTime FromDate, DateTime ToDate)
    {
        var mysqlDataAgent = new MySqlDataAgent();
        return mysqlDataAgent.InventoryReport(FromDate, ToDate);
    }
}

