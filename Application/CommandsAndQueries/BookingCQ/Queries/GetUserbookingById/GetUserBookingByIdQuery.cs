﻿using Application.Dtos.BookingDtos;
using MediatR;

namespace Application.CommandsAndQueries.BookingCQ.Queries.GetUserbookingById
{
    public class GetUserBookingByIdQuery : IRequest<BookingWithPaymentIntentDto>
    {
        public string UserId { get; set; }
        public int BookingId { get; set; }
    }
}
