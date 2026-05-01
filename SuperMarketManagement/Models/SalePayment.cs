using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperMarketManagement.Models;

public partial class SalePayment
{
    [Key]
    public int Id { get; set; }

    public int SaleId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [StringLength(20)]
    public string PaymentMethod { get; set; } = string.Empty;

    public DateTime PaymentDateTime { get; set; }
}
