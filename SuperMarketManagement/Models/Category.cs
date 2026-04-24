using System;
using System.ComponentModel.DataAnnotations;

namespace SuperMarketManagement.Models;

public partial class Category
{
    [Key]
    public int Id { get; set; }

    [StringLength(30)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
