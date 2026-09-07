using System;
using System.Threading.Tasks;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public static class ReturnStatus
    {
        public const string Requested = "Requested";
        public const string Approved = "Approved";
        public const string Inspected = "Inspected";
        public const string RefundPending = "RefundPending";
        public const string Completed = "Completed";
        public const string Rejected = "Rejected";
    }

    public static class ReturnInspectionState
    {
        public const string AwaitingInspection = "AwaitingInspection";
        public const string PassedInspection = "PassedInspection";
        public const string MinorPackagingDamage = "MinorPackagingDamage";
        public const string DamagedInTransit = "DamagedInTransit";
        public const string MissingParts = "MissingParts";
        public const string RejectedInspection = "RejectedInspection";
    }

    public class ReturnOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Order? Order { get; set; }
        public string? CurrentState { get; set; }

        public static ReturnOperationResult Ok(Order order, string message, string? state = null) =>
            new() { Success = true, Order = order, Message = message, CurrentState = state ?? order.RefundStatus };

        public static ReturnOperationResult Fail(string message, Order? order = null) =>
            new() { Success = false, Order = order, Message = message, CurrentState = order?.RefundStatus };
    }

    public interface IReturnRefundService
    {
        /// <summary>
        /// Customer or staff initiates a return request for a delivered order.
        /// </summary>
        Task<ReturnOperationResult> RequestReturnAsync(string orderNumber, string reason, string? customerUserId = null);

        /// <summary>
        /// Administrator approves customer return request. Moves return to Approved state awaiting item inspection.
        /// Never generates fake references or auto-marks payment as refunded.
        /// </summary>
        Task<ReturnOperationResult> ApproveReturnAsync(int orderId, string? adminNotes = null, string? adminUserId = null, string? adminUserName = null);

        /// <summary>
        /// Administrator records the physical condition of returned merchandise.
        /// Moves return to Inspected / RefundPending or Rejected state.
        /// </summary>
        Task<ReturnOperationResult> InspectReturnAsync(int orderId, string inspectionState, string? adminNotes = null, string? adminUserId = null, string? adminUserName = null);

        /// <summary>
        /// Administrator records a verified manual remittance (bank transfer, wallet, or payment card reversal).
        /// Requires genuine transaction reference, validates amount, restocks items idempotently, and audits transition.
        /// </summary>
        Task<ReturnOperationResult> CompleteRefundAsync(
            int orderId,
            string refundTransactionReference,
            decimal? refundAmount = null,
            string? refundMethod = null,
            bool restock = true,
            string? adminNotes = null,
            string? adminUserId = null,
            string? adminUserName = null);

        /// <summary>
        /// Administrator declines the return request or fails the inspection.
        /// </summary>
        Task<ReturnOperationResult> RejectReturnAsync(int orderId, string? inspectionState = null, string? adminNotes = null, string? adminUserId = null, string? adminUserName = null);
    }
}
