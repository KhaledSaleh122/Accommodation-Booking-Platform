using Application.Dtos.AmenityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Delete
{
    internal class DeleteAmenityHandler : IRequestHandler<DeleteAmenityCommand, AmenityDto>
    {
        private readonly IMapper _mapper;
        private readonly IAmenityRepository _repository;

        public DeleteAmenityHandler(IAmenityRepository repository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Amenity, AmenityDto>();
                cfg.CreateMap<AmenityDto, Amenity>();
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<AmenityDto> Handle(DeleteAmenityCommand request, CancellationToken cancellationToken)
        {
            Amenity deletedAmenity;
            try
            {
                var amenity = await _repository.GetByIdAsync(request.Id)
                    ?? throw new NotFoundException("Amenity not found!");
                deletedAmenity = await _repository.DeleteAsync(amenity);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during deleting amenity with id '{request.Id}'.", exception);
            }
            return _mapper.Map<AmenityDto>(deletedAmenity);
        }
    }
}
