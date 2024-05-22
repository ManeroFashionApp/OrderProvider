namespace OrderProvider.Models;

public class OrderRequest
{
    public Guid UserId { get; set; }
    public string UserEmailAddress { get; set; } = null!;
    public List<Guid> Products { get; set; } = [];
    public decimal TotalPrice { get; set; }

    public decimal DeliveryFee { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public string? RecipientCO { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string ZipCode { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;
}
