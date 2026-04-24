using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperMarketManagement.Models;

public partial class StockTransaction
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    [StringLength(20)]
    public string TransactionType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal QuantityChanged { get; set; }

    public DateTime TransactionDateTime { get; set; }
}
