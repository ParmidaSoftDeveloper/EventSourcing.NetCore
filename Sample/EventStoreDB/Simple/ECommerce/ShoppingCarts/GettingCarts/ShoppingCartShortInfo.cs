namespace ECommerce.ShoppingCarts.GettingCarts;

public record ShoppingCartShortInfo
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public int TotalItemsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public int Version { get; set; }

    public ulong LastProcessedPosition { get; set; }
}
