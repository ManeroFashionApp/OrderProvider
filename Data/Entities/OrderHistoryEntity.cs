using Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class OrderHistoryEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public DateTime Created {  get; set; }

    [Required]
    public OrderStatus PreviousStatus { get; set; }

    [Required]
    public OrderStatus NewStatus { get; set;}

    public OrderEntity Order { get; set; } = null!;

}
