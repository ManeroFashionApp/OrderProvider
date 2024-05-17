using System.ComponentModel.DataAnnotations;
using Data.Enums;

namespace Data.Entities;

public class OrderEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public decimal DeliveryFee { get; set; }

    public string? RecipientCO { get; set; } = null!;

    [Required]
    public string Address { get; set;} = null!;

    [Required]
    public string ZipCode { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string Country { get; set; } = null!;

    [Required]
    public OrderStatus Status { get; set; }

    [Required]
    public DateTime Created { get; set; }

    public DateTime LastUpdated { get; set; }

    
    public List<OrderProductEntity> Products { get; set; } = [];
    public List<OrderHistoryEntity> OrderChanges { get; set; } = [];

}
