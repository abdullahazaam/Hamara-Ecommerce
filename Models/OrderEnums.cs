using System;

namespace HamaraCommerce.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        Packed = 3,
        Shipped = 4,
        OutForDelivery = 5,
        Delivered = 6,
        Cancelled = 7,
        Refunded = 8
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Authorized = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4,
        PartiallyRefunded = 5
    }

    public enum PaymentMethodType
    {
        CashOnDelivery,
        SandboxCard,
        CardGateway,
        BankTransfer
    }
}
