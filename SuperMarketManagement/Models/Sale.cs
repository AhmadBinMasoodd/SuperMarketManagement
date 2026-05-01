using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperMarketManagement.Models;

public partial class Sale
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime SaleDateTime { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(20)]
    public string? PaymentMethod { get; set; }

    [StringLength(100)]
    public string? Remarks { get; set; }
}
