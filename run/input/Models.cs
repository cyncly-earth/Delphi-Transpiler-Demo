
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Demo.Domain
{
    /// <summary>Order processing states in the system.</summary>
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Packed = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }

    /// <summary>Represents an entity with a unique identifier.</summary>
    public interface IEntity
    {
        Guid Id { get; set; }
    }

    /// <summary>Base type for customer-like parties.</summary>
    public abstract class Party : IEntity
    {
        public Guid Id { get; set; }

        /// <summary>The legal display name.</summary>
        public required string Name { get; init; }

        /// <summary>Optional contact email.</summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>Optional phone number</summary>
        [DataMember(Name = "phone")]
        public string? PhoneNumber { get; set; }

        /// <summary>Server-side note (never serialized)</summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public string? InternalNote { get; set; }
    }

    /// <summary>Individual customer.</summary>
    public class Customer : Party
    {
        /// <summary>Date the customer registered (local date only).</summary>
        public DateOnly RegisteredOn { get; init; }

        /// <summary>Total loyalty points the customer has.</summary>
        public int LoyaltyPoints { get; set; }

        /// <summary>Optional avatar photo bytes.</summary>
        public byte[]? Avatar { get; set; }
    }

    /// <summary>Corporate seller/merchant.</summary>
    public class Merchant : Party
    {
        /// <summary>Tax registration id.</summary>
        public required string TaxId { get; init; }

        /// <summary>When the merchant was onboarded.</summary>
        public DateTimeOffset OnboardedAt { get; init; }
    }

    /// <summary>Physical address value object.</summary>
    public record Address(
        string Line1,
        string? Line2,
        string City,
        string State,
        string PostalCode,
        string Country);

    /// <summary>Item placed in an order.</summary>
    public record OrderItem(
        string Sku,
        string Title,
        int Quantity,
        decimal UnitPrice)
    {
        /// <summary>Optional discount for this item.</summary>
        public decimal? Discount { get; init; }
    }

    /// <summary>Payment info (example of struct).</summary>
    public struct Payment
    {
        public string Method { get; set; }           // e.g., "Card", "COD"
        public DateTime PaidOnUtc { get; set; }
        public string? Reference { get; set; }
    }

    /// <summary>Shipment info with tuple and dictionary examples.</summary>
    public class Shipment
    {
        public string Carrier { get; set; } = "UPS";
        public string Service { get; set; } = "Ground";

        /// <summary>Tracking details (code, optional url)</summary>
        public (string code, string? url)? Tracking { get; set; }

        /// <summary>Arbitrary metadata (string keys)</summary>
        public Dictionary<string, string> Meta { get; init; } = new();
    }

    /// <summary>The primary order aggregate.</summary>
    public class Order : IEntity
    {
        public Guid Id { get; set; }

        /// <summary>Human-friendly order number.</summary>
        [JsonProperty("orderNo")]
        public required string Number { get; init; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>UTC time when the order was created.</summary>
        public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;

        /// <summary>Optional promised local delivery date.</summary>
        public DateOnly? PromisedDate { get; set; }

        /// <summary>The customer who placed this order.</summary>
        public required Customer Customer { get; init; }

        /// <summary>Items included in the order.</summary>
        public List<OrderItem> Items { get; init; } = new();

        /// <summary>Shipping address.</summary>
        public required Address ShipTo { get; init; }

        /// <summary>Billing address (may be absent and default to ShipTo).</summary>
        public Address? BillTo { get; init; }

        /// <summary>Payments made against this order (array sample).</summary>
        public Payment[] Payments { get; init; } = Array.Empty<Payment>();

        /// <summary>Shipment details.</summary>
        public Shipment? Shipment { get; set; }

        /// <summary>Amounts bucket by currency code (e.g., "USD" -> total).</summary>
        public Dictionary<string, decimal> TotalsByCurrency { get; init; } = new();

        /// <summary>Coupon usage keyed by numeric code (dictionary with number key).</summary>
        public Dictionary<int, string> AppliedCoupons { get; init; } = new();

        /// <summary>Optional time-only window when delivery can occur.</summary>
        public (TimeOnly start, TimeOnly end)? DeliveryWindow { get; set; }

        /// <summary>Arbitrary labels.</summary>
        public HashSet<string> Tags { get; init; } = new();
    }

    /// <summary>Generic paging wrapper.</summary>
    public class Paged<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
    }

    /// <summary>Envelope pattern for API results.</summary>
    public class Response<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
        public Dictionary<string, string[]>? Errors { get; init; }
    }

    /// <summary>Query DTO with various types.</summary>
    public class OrderQuery
    {
        public string? Search { get; init; }

        [JsonPropertyName("from")]
        public DateTime? FromUtc { get; init; }

        [JsonPropertyName("to")]
        public DateTime? ToUtc { get; init; }

        public OrderStatus[]? Statuses { get; init; }

        /// <summary>Paging tuple (page, size).</summary>
        public (int page, int size) Paging { get; init; } = (1, 20);
    }

    /// <summary>Example of a type with overridden JSON names.</summary>
    public class InventoryItem : IEntity
    {
        public Guid Id { get; set; }

        [JsonPropertyName("sku")]
        public required string Sku { get; init; }

        [JsonProperty("title")]
        public required string Name { get; init; }

        [DataMember(Name = "qty")]
        public int QuantityOnHand { get; set; }

        /// <summary>Null means unknown or N/A.</summary>
        public TimeSpan? ReplenishmentLeadTime { get; set; }

        /// <summary>Maps to enum in TS.</summary>
        public ItemCondition Condition { get; set; } = ItemCondition.New;
    }

    public enum ItemCondition { New = 1, Used = 2, Refurbished = 3 }

    /// <summary>Search results combining multiple types.</summary>
    public class SearchResults
    {
        public required Paged<Order> Orders { get; init; }
        public required Paged<InventoryItem> Inventory { get; init; }

        /// <summary>Key/value highlights.</summary>
        public Dictionary<string, string[]> Highlights { get; init; } = new();
    }

    /// <summary>A small aggregate to touch arrays, lists, and nested generics.</summary>
    public class DashboardSnapshot
    {
        public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
        public Response<Paged<Order>> RecentOrders { get; init; } = new() { Success = true, Data = new Paged<Order> { Items = Array.Empty<Order>(), Page = 1, PageSize = 10, TotalCount = 0 } };
        public Response<Paged<InventoryItem>> LowStock { get; init; } = new() { Success = true, Data = new Paged<InventoryItem> { Items = Array.Empty<InventoryItem>(), Page = 1, PageSize = 10, TotalCount = 0 } };
        public string[] Notices { get; init; } = Array.Empty<string>();
    }
}
