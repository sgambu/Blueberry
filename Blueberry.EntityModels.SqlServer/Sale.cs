using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Blueberry.EntityModels.SaleItem;

namespace Blueberry.EntityModels;

[Table("Sale")]
public partial class Sale
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    [StringLength(20)]
    public string? TransNumber { get; set; }

    [StringLength(20)]
    public string? AuthNumber { get; set; }

    public int MerchantId { get; set; }

    [ForeignKey("MerchantId")]
    [InverseProperty("Sales")]
    public virtual Merchant Merchant { get; set; } = null!;

    [InverseProperty("Sale")]
    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
public class CreateSaleRequest
{
    //public int CustomerId { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    //public string TransNumber { get; set; }
    //public string? AuthNumber { get; set; }

    public List<CreateSaleItemRequest> Items { get; set; } = [];
}