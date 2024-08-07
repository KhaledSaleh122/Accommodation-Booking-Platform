using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CommandsAndQueries.BookingCQ.Commands.GenerateReport
{
    public class GenerateConfirmationReportQuery : IRequest<byte[]>
    {
        public string UserId { get; set; }
        public int BookingId { get; set; }
    }
}
