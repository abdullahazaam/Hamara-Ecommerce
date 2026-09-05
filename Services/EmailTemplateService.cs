using System.Text;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public interface IEmailTemplateService
    {
        string GenerateAccountVerificationEmail(string customerName, string confirmationUrl);
        string GeneratePasswordResetEmail(string customerName, string resetUrl);
        string GenerateOrderConfirmationEmail(Order order);
        string GenerateOrderStatusUpdateEmail(Order order, string previousStatus, string newStatus);
        string GenerateOrderCancellationEmail(Order order, string reason);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IShippingTaxService _shippingTaxService;

        public EmailTemplateService(IShippingTaxService shippingTaxService)
        {
            _shippingTaxService = shippingTaxService;
        }

        public string GenerateAccountVerificationEmail(string customerName, string confirmationUrl)
        {
            var content = $@"
                <h2 style=""color: #0f172a; margin-top: 0;"">Welcome to Hamara Commerce, {System.Net.WebUtility.HtmlEncode(customerName)}!</h2>
                <p style=""color: #475569; font-size: 15px; line-height: 1.6;"">
                    Thank you for creating your customer account. To activate your full account features, track orders, and securely manage your saved addresses, please verify your email address.
                </p>
                <div style=""text-align: center; margin: 32px 0;"">
                    <a href=""{confirmationUrl}"" style=""background: linear-gradient(135deg, #2563eb 0%, #7c3aed 100%); color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 50px; font-weight: bold; font-size: 15px; display: inline-block;"">
                        Verify Email Address
                    </a>
                </div>
                <p style=""color: #94a3b8; font-size: 13px;"">If you didn't create this account, please disregard this message.</p>";

            return WrapInBaseLayout("Verify Your Hamara Commerce Account", content);
        }

        public string GeneratePasswordResetEmail(string customerName, string resetUrl)
        {
            var content = $@"
                <h2 style=""color: #0f172a; margin-top: 0;"">Password Reset Request</h2>
                <p style=""color: #475569; font-size: 15px; line-height: 1.6;"">
                    Hello {System.Net.WebUtility.HtmlEncode(customerName)}, we received a request to reset your customer account password. Click the button below to choose a new secure password.
                </p>
                <div style=""text-align: center; margin: 32px 0;"">
                    <a href=""{resetUrl}"" style=""background: #0f172a; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 50px; font-weight: bold; font-size: 15px; display: inline-block;"">
                        Reset Password
                    </a>
                </div>
                <p style=""color: #94a3b8; font-size: 13px;"">This link will expire in 2 hours for security reasons. If you did not request a password reset, please ignore this email.</p>";

            return WrapInBaseLayout("Reset Your Account Password", content);
        }

        public string GenerateOrderConfirmationEmail(Order order)
        {
            var itemsHtml = new StringBuilder();
            foreach (var item in order.Items)
            {
                itemsHtml.Append($@"
                    <tr>
                        <td style=""padding: 10px 0; border-bottom: 1px solid #e2e8f0;"">
                            <strong style=""color: #0f172a; font-size: 14px;"">{System.Net.WebUtility.HtmlEncode(item.ProductTitle)}</strong>
                            <div style=""color: #64748b; font-size: 12px;"">Qty: {item.Quantity} &times; {_shippingTaxService.FormatCurrency(item.UnitPrice)}</div>
                        </td>
                        <td style=""padding: 10px 0; border-bottom: 1px solid #e2e8f0; text-align: right; font-weight: bold; color: #0f172a;"">
                            {_shippingTaxService.FormatCurrency(item.TotalPrice)}
                        </td>
                    </tr>");
            }

            var content = $@"
                <div style=""text-align: center; margin-bottom: 24px;"">
                    <span style=""background: #ecfdf5; color: #065f46; padding: 6px 16px; border-radius: 50px; font-weight: bold; font-size: 13px;"">Order Confirmed</span>
                    <h2 style=""color: #0f172a; margin-top: 12px; margin-bottom: 4px;"">Thank You For Your Order!</h2>
                    <p style=""color: #64748b; margin: 0; font-size: 14px;"">Order #{order.OrderNumber}</p>
                </div>

                <div style=""background: #f8fafc; border-radius: 12px; padding: 16px; margin-bottom: 24px; font-size: 13px;"">
                    <div style=""margin-bottom: 6px;""><strong>Tracking Number:</strong> <span style=""font-family: monospace; color: #2563eb;"">{order.TrackingNumber}</span></div>
                    <div style=""margin-bottom: 6px;""><strong>Payment Method:</strong> {order.PaymentMethod}</div>
                    <div><strong>Delivery Address:</strong> {System.Net.WebUtility.HtmlEncode(order.ShippingAddress)}, {System.Net.WebUtility.HtmlEncode(order.City)}, {System.Net.WebUtility.HtmlEncode(order.State)}</div>
                </div>

                <table style=""width: 100%; border-collapse: collapse; margin-bottom: 24px;"">
                    <thead>
                        <tr>
                            <th style=""text-align: left; padding-bottom: 8px; border-bottom: 2px solid #cbd5e1; color: #475569; font-size: 12px; text-transform: uppercase;"">Item</th>
                            <th style=""text-align: right; padding-bottom: 8px; border-bottom: 2px solid #cbd5e1; color: #475569; font-size: 12px; text-transform: uppercase;"">Amount</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td style=""padding: 6px 0; color: #64748b; font-size: 13px;"">Subtotal:</td>
                            <td style=""padding: 6px 0; text-align: right; color: #0f172a; font-size: 13px;"">{_shippingTaxService.FormatCurrency(order.Subtotal)}</td>
                        </tr>
                        {(order.DiscountAmount > 0 ? $@"<tr><td style=""padding: 6px 0; color: #10b981; font-size: 13px;"">Discount:</td><td style=""padding: 6px 0; text-align: right; color: #10b981; font-size: 13px;"">-{_shippingTaxService.FormatCurrency(order.DiscountAmount)}</td></tr>" : "")}
                        <tr>
                            <td style=""padding: 6px 0; color: #64748b; font-size: 13px;"">Tax (GST):</td>
                            <td style=""padding: 6px 0; text-align: right; color: #0f172a; font-size: 13px;"">{_shippingTaxService.FormatCurrency(order.TaxAmount)}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 6px 0; color: #64748b; font-size: 13px;"">Shipping:</td>
                            <td style=""padding: 6px 0; text-align: right; color: #0f172a; font-size: 13px;"">{(order.ShippingFee == 0 ? "FREE" : _shippingTaxService.FormatCurrency(order.ShippingFee))}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 12px 0 0 0; font-size: 16px; font-weight: bold; color: #0f172a; border-top: 1px solid #cbd5e1;"">Total:</td>
                            <td style=""padding: 12px 0 0 0; text-align: right; font-size: 16px; font-weight: bold; color: #2563eb; border-top: 1px solid #cbd5e1;"">{_shippingTaxService.FormatCurrency(order.TotalAmount)}</td>
                        </tr>
                    </tfoot>
                </table>";

            return WrapInBaseLayout($"Order Confirmation #{order.OrderNumber}", content);
        }

        public string GenerateOrderStatusUpdateEmail(Order order, string previousStatus, string newStatus)
        {
            var content = $@"
                <h2 style=""color: #0f172a; margin-top: 0;"">Order #{order.OrderNumber} Update</h2>
                <p style=""color: #475569; font-size: 15px; line-height: 1.6;"">
                    Your order status has changed to <strong style=""color: #2563eb;"">{newStatus}</strong>.
                </p>
                <div style=""background: #f8fafc; border-radius: 12px; padding: 16px; margin: 24px 0; font-size: 14px;"">
                    <div><strong>Tracking Reference:</strong> <span style=""font-family: monospace; color: #2563eb;"">{order.TrackingNumber}</span></div>
                    <div><strong>Courier:</strong> {order.ShippingMethod}</div>
                    <div><strong>Destination:</strong> {System.Net.WebUtility.HtmlEncode(order.City)}, {System.Net.WebUtility.HtmlEncode(order.State)}</div>
                </div>";

            return WrapInBaseLayout($"Order #{order.OrderNumber} Status: {newStatus}", content);
        }

        public string GenerateOrderCancellationEmail(Order order, string reason)
        {
            var content = $@"
                <h2 style=""color: #ef4444; margin-top: 0;"">Order #{order.OrderNumber} Cancelled</h2>
                <p style=""color: #475569; font-size: 15px; line-height: 1.6;"">
                    Your order #{order.OrderNumber} has been cancelled.
                </p>
                <div style=""background: #fef2f2; border-left: 4px solid #ef4444; padding: 12px; margin: 20px 0; font-size: 14px; color: #991b1b;"">
                    <strong>Reason / Note:</strong> {System.Net.WebUtility.HtmlEncode(reason)}
                </div>
                <p style=""color: #475569; font-size: 14px;"">If payment was already made, our financial team has initiated a full refund via your original payment method.</p>";

            return WrapInBaseLayout($"Order #{order.OrderNumber} Cancellation Notice", content);
        }

        private static string WrapInBaseLayout(string title, string bodyHtml)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{System.Net.WebUtility.HtmlEncode(title)}</title>
</head>
<body style=""margin: 0; padding: 20px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9;"">
    <div style=""max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.06);"">
        <div style=""background-color: #0f172a; padding: 24px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 22px; letter-spacing: -0.5px;"">Hamara <span style=""color: #3b82f6;"">Commerce</span></h1>
        </div>
        <div style=""padding: 32px;"">
            {bodyHtml}
        </div>
        <div style=""background-color: #f8fafc; padding: 20px; text-align: center; border-top: 1px solid #e2e8f0; font-size: 12px; color: #94a3b8;"">
            &copy; {System.DateTime.UtcNow.Year} Hamara Commerce (Pvt) Ltd. All rights reserved.<br />
            Plaza 45, Main Boulevard, Gulberg III, Lahore, Pakistan.
        </div>
    </div>
</body>
</html>";
        }
    }
}
