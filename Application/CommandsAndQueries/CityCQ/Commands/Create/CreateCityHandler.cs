using Application.Dtos.CityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.CommandsAndQueries.CityCQ.Commands.Create
{
    internal class CreateCityHandler : IRequestHandler<CreateCityCommand, CityDto>
    {
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imageRepository;
        private readonly ITransactionService _transactionService;

        public CreateCityHandler(
            ICityRepository cityRepository,
            IImageService imageRepository, 
            ITransactionService transactionService)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<CreateCityCommand, City>();
            });
            _mapper = configuration.CreateMapper();
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        public async Task<CityDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
        {
            var city = _mapper.Map<City>(request);
            try
            {
                var isCityExist = await _cityRepository.DoesCityExistInCountryAsync(
                    request.Name, request.Country
                );
                if (isCityExist)
                    throw new ErrorException("A city with this name already exists in the country.") { 
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var isPostExist = await _cityRepository.DoesPostOfficeExistsAsync(request.PostOffice);
                if (isPostExist)
                    throw new ErrorException("This post office exists in a city.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var storePath = "CityThumbnails";
                var thumnailName = Guid.NewGuid().ToString();
                city.Thumbnail =$"{storePath}/{thumnailName}{Path.GetExtension(request.Thumbnail.FileName)}";
                await _transactionService.BeginTransactionAsync();
                _imageRepository.UploadFile(request.Thumbnail, $"{storePath}", thumnailName);
                await _cityRepository.CreateAsync(city);
                await _transactionService.CommitTransactionAsync();
                var cityDto = _mapper.Map<CityDto>(city);
                return cityDto;
            }
            catch (ErrorException) {
                throw;
            }
            catch (Exception exception)
            {
                await _transactionService.RollbackTransactionAsync();
                _imageRepository.DeleteFile(city.Thumbnail, true);
                throw new ErrorException($"Error during creating new city.", exception);
            }
        }
    }
}
