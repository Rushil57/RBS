using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new InventoryAgent().GetManufacturers());
        }
        
        [HttpPost]
        public IActionResult GetInventories(Inventory inventory)
        {
            return Ok(new InventoryAgent().GetInventories(inventory.ManufacturerId));
        }
        [HttpGet("{productNo}")]
        public IActionResult GetInventory(string productNo)
        {
            return Ok(new InventoryAgent().GetInventoryByProductNumber(productNo));
        }
        [HttpPut]
        public IActionResult SaveInventories(IEnumerable<Inventory> inventories)
        { 
            return Ok(new InventoryAgent().SaveInventories(inventories));
        }
    }
}