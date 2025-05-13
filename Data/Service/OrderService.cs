using BookLibraryStore.Data.Service.IService;
using BookLibraryStore.Hubs;
using BookLibraryStore.Models.EntityModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryStore.Data.Service
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderBroadcastHub> _hubContext;

        public OrderService(ApplicationDbContext context, IHubContext<OrderBroadcastHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task BroadcastOrderAsync(Order order)
        {
            // Generate message
            var message = GenerateMessage(order);

            // Save to DB
            var broadcast = new OrderBroadcast
            {
                OrderId = order.Id,
                Message = message,
                IsDisplayed = true,
                DisplayDuration = 30
            };
            _context.OrderBroadcasts.Add(broadcast);
            await _context.SaveChangesAsync();

            // Push real-time
            await _hubContext.Clients.All.SendAsync("ReceiveOrderBroadcast", message);
        }

        private string GenerateMessage(Order order)
        {
            var itemCount = _context.OrderItems.Count(oi => oi.OrderId == order.Id);
            switch (itemCount)
            {
                case 1:
                {
                    var bookTitle = _context.OrderItems.Include(orderItem => orderItem.Book).First(oi => oi.OrderId == order.Id).Book.Title;
                    return $"A customer just purchased '{bookTitle}'!";
                }
                case <= 3:
                {
                    var bookTitle = _context.OrderItems.Include(orderItem => orderItem.Book).First(oi => oi.OrderId == order.Id).Book.Title;
                    return $"Someone just bought '{bookTitle}' and {itemCount - 1} other books!";
                }
                default:
                    return $"A book enthusiast just completed an order of {itemCount} books!";
            }
        }
    }
}
