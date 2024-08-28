using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IInvoiceGeneraterService
    {
        byte[] GenerateInvoicePdf(Booking booking, User user, Hotel hotel);
    }
}
