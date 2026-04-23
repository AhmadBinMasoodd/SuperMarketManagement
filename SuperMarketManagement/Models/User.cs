using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SuperMarketManagement.Models;

public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(30)]
    public string Name { get; set; } = null!;

    public int Age { get; set; }

    [StringLength(8)]
    public string Gender { get; set; } = null!;

    [StringLength(40)]
    public string Address { get; set; } = null!;

    [StringLength(20)]
    public string Role { get; set; } = null!;

    [StringLength(10)]
    public string Username { get; set; } = null!;

    [StringLength(10)]
    public string Password { get; set; } = null!;

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? Salary { get; set; }
}
