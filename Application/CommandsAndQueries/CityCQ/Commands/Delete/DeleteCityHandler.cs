using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.CityCQ.Commands.Delete
{
    internal class DeleteCityHandler : IRequestHandler<DeleteCityCommand, CityDto>
    {
        private readonly IMapper _mapper;
        private readonly ICityRepository _repository;
        private readonly IImageService _imageRepository;
        private readonly ITransactionService _transactionService;

        public DeleteCityHandler(
            ICityRepository repository, 
            IImageService imageRepository,
            ITransactionService transactionService)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<CityDto, City>();
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        public async Task<CityDto> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
        {
            City deletedCity;
            try
            {
                var city = await _repository.GetByIdAsync(request.Id) ?? throw new NotFoundException();
                await _transactionService.BeginTransactionAsync();
                deletedCity = await _repository.DeleteAsync(city);
                _imageRepository.DeleteFile(city.Thumbnail);
                await _transactionService.CommitTransactionAsync();
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                await _transactionService.RollbackTransactionAsync();
                throw new ErrorException($"Error during deleting city with id '{request.Id}'.", exception);
            }
            return _mapper.Map<CityDto>(deletedCity);
        }
    }
}
