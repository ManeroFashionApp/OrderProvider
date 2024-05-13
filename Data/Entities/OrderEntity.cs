﻿using System.ComponentModel.DataAnnotations;
using Data.Enums;

namespace Data.Entities;

public class OrderEntity
{
    [Key]
    public Guid Id { get; set; }

    //UserId from UserProvider or token
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public decimal DeliveryFee { get; set; }

    [Required]
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