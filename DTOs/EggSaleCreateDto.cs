using FarmManagement.API.Models;
using System;

namespace FarmManagement.API.DTOs
{
    public class EggSaleCreateDto
    {
        public int WarehouseId { get; set; }
        public int TraderId { get; set; }
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal PaidAmount { get; set; }
        public string? Notes { get; set; }
        public EggQualityType EggQuality { get; set; }
    }

    public class EggSaleUpdatePriceDto
    {
        public decimal UnitPrice { get; set; }
    }
}