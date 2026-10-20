using Blueberry.EntityModels;


namespace Blueberry.Blazor;

public class SelectedMerchantState
{
    public Merchant? SelectedMerchant { get; set; }
}

public class ProductModification
{
    public int ProductId { get; set; }
    public decimal NewPrice { get; set; }
    public int QuantityToAdd { get; set; }
}
