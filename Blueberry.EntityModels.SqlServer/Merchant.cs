using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blueberry.EntityModels;

[Table("Merchant")]
public partial class Merchant
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string? Adresse { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [InverseProperty("Merchant")]
    public virtual ICollection<MerchantProduct> MerchantProducts { get; set; } = new List<MerchantProduct>();

    [InverseProperty("Merchant")]
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    [ForeignKey("MerchantId")]
    [InverseProperty("Merchants")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
