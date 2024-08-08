using Application.Dtos.HotelDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Delete
{
    internal class DeleteHotelHandler : IRequestHandler<DeleteHotelCommand, HotelMinDto>
    {
        private readonly IMapper _mapper;
        private readonly IHotelRepository _repository;
        private readonly IImageService _imageRepository;
        private readonly ITransactionService _transactionService;

        public DeleteHotelHandler(
            IHotelRepository repository,
            IImageService imageRepository,
            ITransactionService transactionService)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Hotel, HotelMinDto>()
                    .ForMember(dest => dest.City,
                       opt =>
                            opt.MapFrom(src => src.City.Name)
                    );
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        public async Task<HotelMinDto> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
        {
            Hotel deletedHotel;
            try
            {
                var (hotel, avgReviews) = await _repository.GetByIdAsync(request.Id) ?? throw new NotFoundException();
                await _transactionService.BeginTransactionAsync();
                deletedHotel = await _repository.DeleteAsync(hotel);
                deletedHotel.City = hotel.City;
                _imageRepository.DeleteFile(hotel.Thumbnail);
                foreach (var image in hotel.Images)
                {
                    _imageRepository.DeleteFile(image.Path);
                }
                await _transactionService.CommitTransactionAsync();
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                await _transactionService.RollbackTransactionAsync();
                throw new ErrorException($"Error during deleting hotel with id '{request.Id}'.", exception);
            }
            return _mapper.Map<HotelMinDto>(deletedHotel);
        }
    }
}
