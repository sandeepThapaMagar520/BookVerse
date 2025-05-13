using BookLibraryStore.Models.EntityModel;

namespace BookLibraryStore.Data.Service.IService
{
    public interface IOrderService
    {
        public Task BroadcastOrderAsync(Order order);
    }
}
