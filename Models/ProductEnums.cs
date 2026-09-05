namespace HamaraCommerce.Models
{
    public enum ProductStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum InventoryMovementType
    {
        PurchaseRestock = 1,
        ManualAdjustment = 2,
        OrderDeduction = 3,
        OrderCancellationRestoration = 4,
        ReturnRestoration = 5,
        DamageWriteOff = 6
    }
}
