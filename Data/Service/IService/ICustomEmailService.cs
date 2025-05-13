using BookLibraryStore.Models.ServiceModel;

namespace BookLibraryStore.Data.Service.IService
{
    public interface ICustomEmailService
    {
        public Task SendEmailAsync(MailRequest mailRequest);
    }
}
