using BookLibraryStore.Models.EntityModel;
using BookLibraryStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookLibraryStore.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using BookLibraryStore.Data.Service.IService;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookLibraryStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IOrderService _orderService;

        public OrderController(ApplicationDbContext context, IEmailSender emailSender, IOrderService orderService)
        {
            _context = context;
            _emailSender = emailSender;
            _orderService = orderService;
        }
        public IActionResult Index(string statusFilter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == userId);
            var isAdmin = User.IsInRole("Admin");

            // Base query
            var query = _context.Orders
                .Include(o => o.ApplicationUser)
                .AsQueryable();

            // If not admin, filter user's own orders
            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == userId);
            }

            // Filter by status if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // For dropdown of unique status values
            var statusList = _context.Orders
                .Select(o => o.Status)
                .Distinct()
                .ToList();

            ViewBag.StatusList = statusList;

            return View(orders);
        }


        [Authorize]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == userId);

            // Get cart items of user
            var cartItems = _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["error"] = "Your cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            // Calculate subtotal
            var subtotal = cartItems.Sum(item => item.UnitPrice * item.Quantity);

            // Apply discounts logic
            decimal bulkDiscount = 0;
            decimal loyaltyDiscount = 0;

            // Bulk discount (5% if 5 or more books)
            var totalBooks = cartItems.Sum(c => c.Quantity);
            var is5PercentApplied = false;
            var is10PercentApplied = false;

            if (totalBooks >= 5)
            {
                bulkDiscount = subtotal * 0.05m;
                is5PercentApplied = true;
            }

            // Loyalty discount (check user's completed orders)
            var hasLoyaltyDiscount = user is { HasStackableDiscount: true };

            if (hasLoyaltyDiscount)
            {
                loyaltyDiscount = subtotal * 0.10m;
                is10PercentApplied = true;
                user!.HasStackableDiscount = false;
                await _context.SaveChangesAsync();
            }

            var totalDiscount = bulkDiscount + loyaltyDiscount;
            var grandTotal = subtotal - totalDiscount;

            // Create order
            var newOrder = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = SD.OrderPending,
                ClaimCode = ClaimCodeGenerator.GenerateClaimCode(),

                Subtotal = subtotal,
                BulkDiscountAmount = bulkDiscount,
                LoyaltyDiscountAmount = loyaltyDiscount,
                TotalDiscountAmount = totalDiscount,
                GrandTotal = grandTotal,

                Is5PercentDiscountApplied = is5PercentApplied,
                Is10PercentDiscountApplied = is10PercentApplied
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = newOrder.Id,
                    BookId = cartItem.BookId,
                    UnitPrice = cartItem.UnitPrice,
                    Quantity = cartItem.Quantity
                };
                _context.OrderItems.Add(orderItem);
            }
            await _context.SaveChangesAsync();

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Email send logic
            if (user.EmailConfirmed)
            {
                var orderItems = _context.OrderItems
                    .Include(o => o.Book)
                    .Where(o => o.OrderId == newOrder.Id)
                    .ToList();
                var emailBody = GenerateOrderSuccessEmail(newOrder, orderItems);

                await _emailSender.SendEmailAsync(
                    email:user.Email!, 
                    subject:$"Your Order Confirmation (Claim Code: {newOrder.ClaimCode})",
                    htmlMessage:emailBody
                    );
            }

            TempData["success"] = $"Order placed successfully! Your claim code is {newOrder.ClaimCode}";

            return RedirectToAction("OrderSuccess", new { orderId = newOrder.Id });
        }

        public IActionResult OrderSuccess(int orderId)
        {
            var order = _context.Orders.Include(o => o.ApplicationUser).FirstOrDefault(o => o.Id == orderId);

            var orderItems = _context.OrderItems
                .Include(oi => oi.Book)
                .Where(oi => oi.OrderId == orderId)
                .ToList();

            ViewBag.OrderItems = orderItems;
            return View(order);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult PickupPage()
        {
            return View();
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> ProcessPickup(string membershipId, string claimCode, string actionType)
        {
            // Find user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.ApplicationUsers.FirstOrDefault(u => u.MembershipId == membershipId);

            if (user == null)
            {
                TempData["error"] = "Member not found.";
                return RedirectToAction("PickupPage");
            }

            // Find order
            var order = _context.Orders.FirstOrDefault(o => o.UserId == user.Id && o.ClaimCode == claimCode);
            if (order == null)
            {
                TempData["error"] = "Invalid claim code.";
                return RedirectToAction("PickupPage");
            }

            // Check if already completed or cancelled
            if (order.Status == "Completed" || order.Status == "Cancelled")
            {
                TempData["error"] = "Order is already " + order.Status + ".";
                return RedirectToAction("PickupPage");
            }

            if (actionType == "Complete")
            {
                // Complete order
                order.Status = "Completed";
                order.CompletedDate = DateTime.UtcNow;

                // Update user's successful order count
                user.SuccessfulOrderCount += 1;

                // Check if multiple of 10
                if (user.SuccessfulOrderCount % 10 == 0)
                {
                    user.HasStackableDiscount = true;
                }

                // Broad cast message 
                await _orderService.BroadcastOrderAsync(order);

                TempData["success"] = "Order marked as completed.";
            }
            else if (actionType == "Cancel")
            {
                // Cancel order
                order.Status = "Cancelled";
                order.CancellationDate = DateTime.UtcNow;

                TempData["success"] = "Order cancelled.";
            }
            else
            {
                TempData["error"] = "Invalid action.";
                return RedirectToAction("PickupPage");
            }

            // Save changes
            await _context.SaveChangesAsync();

            return RedirectToAction("PickupPage");
        }

        [Authorize(Roles = SD.Role_Customer)]
        public async Task<IActionResult> ReOrder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == userId);

            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = SD.OrderPending;

                _context.Update(order);
                await _context.SaveChangesAsync();

                // Email send logic
                if (user.EmailConfirmed)
                {
                    var orderItems = _context.OrderItems
                        .Include(o => o.Book)
                        .Where(o => o.OrderId == order.Id)
                        .ToList();
                    var emailBody = GenerateOrderSuccessEmail(order, orderItems);

                    await _emailSender.SendEmailAsync(
                        email: user.Email!,
                        subject: $"Your Order Confirmation (Claim Code: {order.ClaimCode})",
                        htmlMessage: emailBody
                    );
                }

                TempData["success"] = "Re Order success";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_Customer)]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (order.UserId != userId)
            {
                TempData["error"] = "You are not authorized to cancel this order.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == SD.OrderCancelled)
            {
                TempData["info"] = "Order is already cancelled.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = SD.OrderCancelled;
            _context.Orders.Update(order);
            _context.SaveChanges();

            TempData["success"] = "Order cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }


        public string GenerateOrderSuccessEmail(Order order, List<OrderItem> items)
        {
            var sb = new StringBuilder();

            sb.Append($@"
                <h2>Order Confirmation - Claim Code: {order.ClaimCode}</h2>
                <p>Membership ID: {order.ApplicationUser?.MembershipId}</p>
                <p>Hi {order.ApplicationUser?.FirstName} {order.ApplicationUser?.LastName},</p>
                <p>Thank you for your order placed on {order.OrderDate.ToString("dd MMM yyyy")}.</p>

                <h3>Order Summary:</h3>
                <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                <tr>
                    <th>Book</th>
                    <th>Unit Price</th>
                    <th>Quantity</th>
                    <th>Total</th>
                </tr>");

            foreach (var item in items)
            {
                sb.Append($@"
                    <tr>
                        <td>{item.Book.Title}</td>
                        <td>Rs. {item.UnitPrice}</td>
                        <td>{item.Quantity}</td>
                        <td>Rs. {item.UnitPrice * item.Quantity}</td>
                    </tr>");
            }

            sb.Append($@"
                </table>
                <br/>
                <p><strong>Subtotal:</strong> Rs. {order.Subtotal}</p>
                <p><strong>Discounts:</strong> Rs. {order.TotalDiscountAmount}</p>
                <p><strong>Grand Total:</strong> Rs. {order.GrandTotal}</p>

                <br/>
                <p><strong>Your Claim Code:</strong> {order.ClaimCode}</p>
                <p>Please show this code at store pickup to claim your books!</p>

                <br/>
                <p>Thank you for shopping with us.</p>
                ");

            return sb.ToString();
        }

    }
}
