using MediatR;

namespace Application.CommandsAndQueries.BookingCQ.Commands.GenerateReport
{
    public class GenerateConfirmationReportQuery : IRequest<byte[]>
    {
        public string UserId { get; set; }
        public int BookingId { get; set; }
    }
}
