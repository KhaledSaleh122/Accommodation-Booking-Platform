using Application.Dtos.RoomDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.RoomCQ.Commands.Delete
{
    public class DeleteRoomHandler : IRequestHandler<DeleteRoomCommand, RoomDto>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imageRepository;
        private readonly IHotelRoomRepository _hotelRoomRepository;
        private readonly ITransactionService _transactionService;

        public DeleteRoomHandler(
            IHotelRepository hotelRepository,
            IImageService imageRepository,
            IHotelRoomRepository hotelRoomRepository
            , ITransactionService transactionService)
        {
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Room, RoomDto>()
                  .ForMember(dest => dest.Images, opt =>
                    opt.MapFrom(
                      src => src.Images.Select(x => x.Path).ToList()
                    )
                  );
            });

            _mapper = configuration.CreateMapper();
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _hotelRoomRepository = hotelRoomRepository ?? throw new ArgumentNullException(nameof(hotelRoomRepository));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        public async Task<RoomDto> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.HotelId) 
                    ?? throw new NotFoundException("Hotel not found!");
                var room = await _hotelRoomRepository.GetHotelRoomAsync(request.HotelId, request.RoomNumber) 
                    ?? throw new NotFoundException("Room not found");
                await _transactionService.BeginTransactionAsync();
                await _hotelRoomRepository.DeleteRoomAsync(room);
                _imageRepository.DeleteFile(room.Thumbnail);
                foreach (var image in room.Images)
                {
                    _imageRepository.DeleteFile(image.Path);
                }
                await _transactionService.CommitTransactionAsync();
                return _mapper.Map<RoomDto>(room);
            }
            catch (NotFoundException) {
                throw;
            }
            catch (Exception exception)
            {
                await _transactionService.RollbackTransactionAsync();
                throw new ErrorException(
                    $"Error during deleting Room with room number " +
                    $"'{request.RoomNumber}' from hotel with id '{request.HotelId}'.",
                    exception
                );
            }
        }
    }
}
