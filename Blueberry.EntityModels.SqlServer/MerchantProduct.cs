using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blueberry.EntityModels;

[PrimaryKey("MerchantId", "ProductId")]
[Table("MerchantProduct")]
public partial class MerchantProduct
{
    [Key]
    public int MerchantId { get; set; }

    [Key]
    public int ProductId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    [ForeignKey("MerchantId")]
    [InverseProperty("MerchantProducts")]
    public virtual Merchant Merchant { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("MerchantProducts")]
    public virtual Product Product { get; set; } = null!;
}
