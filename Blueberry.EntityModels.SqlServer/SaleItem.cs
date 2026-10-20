using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blueberry.EntityModels;

[Table("SaleItem")]
public partial class SaleItem
{
    [Key]
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal UnitPrice { get; set; }

    [Column("TPSAmount", TypeName = "decimal(10, 2)")]
    public decimal? Tpsamount { get; set; }

    [Column("TVQAmount", TypeName = "decimal(10, 2)")]
    public decimal? Tvqamount { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("SaleItems")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("SaleId")]
    [InverseProperty("SaleItems")]
    public virtual Sale Sale { get; set; } = null!;

}
public class CreateSaleItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal TpsAmount { get; set; }
    public decimal TvqAmount { get; set; }
}