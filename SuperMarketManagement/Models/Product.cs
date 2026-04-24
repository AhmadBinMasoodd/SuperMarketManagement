using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperMarketManagement.Models;

public partial class Product
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PurchasePrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SalePrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; set; }

    [StringLength(20)]
    public string Unit { get; set; } = null!;
}
