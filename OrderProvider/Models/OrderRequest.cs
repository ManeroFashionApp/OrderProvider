using System.ComponentModel.DataAnnotations;

namespace OrderProvider.Models;

public class OrderRequest
{
    public List<string> Products { get; set; } = [];
    public decimal TotalPrice { get; set; }

    public decimal DeliveryFee { get; set; }

    public string? RecipientCO { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string ZipCode { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;
}
