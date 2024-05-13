using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class OrderProductEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public int Count { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public OrderEntity Order { get; set; } = null!;
}
