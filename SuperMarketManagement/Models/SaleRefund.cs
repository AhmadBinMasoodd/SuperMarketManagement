using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperMarketManagement.Models;

public partial class SaleRefund
{
    [Key]
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RefundAmount { get; set; }

    public DateTime RefundDateTime { get; set; }

    [StringLength(100)]
    public string? Remarks { get; set; }
}
