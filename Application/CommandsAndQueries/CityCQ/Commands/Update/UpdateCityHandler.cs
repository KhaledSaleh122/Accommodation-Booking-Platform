using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.CityCQ.Commands.Update
{
    internal class UpdateCityHandler : IRequestHandler<UpdateCityCommand, CityDto>
    {
        private readonly IMapper _mapper;
        private readonly ICityRepository _repository;
        private readonly IImageService _imageService;
        private readonly ITransactionService _transactionService;

        public UpdateCityHandler(ICityRepository repository, IImageService imageService, ITransactionService transactionService)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<UpdateCityCommand, City>();
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }
        public async Task<CityDto> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
        {
            City? updatedCity = null;
            try
            {
                var city = await _repository.GetByIdAsync(request.id) ?? throw new NotFoundException($"Not Found");
                if (!city.Name.Equals(request.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    var isCityExist = await _repository.DoesCityExistInCountryAsync(
                        request.Name, request.Country
                    );
                    if (isCityExist)
                        throw new ErrorException("A city with this name already exists in the country.")
                        {
                            StatusCode = StatusCodes.Status409Conflict
                        };
                }
                if (!city.PostOffice.Equals(request.PostOffice, StringComparison.CurrentCultureIgnoreCase))
                {
                    var isPostExist = await _repository.DoesPostOfficeExistsAsync(request.PostOffice);
                    if (isPostExist)
                        throw new ErrorException("This post office exists in a city.")
                        {
                            StatusCode = StatusCodes.Status409Conflict
                        };
                }

                updatedCity = _mapper.Map(request,city);
                var storePath = "CityThumbnails";
                var thumnailName = Guid.NewGuid().ToString();
                updatedCity.Thumbnail = $"{storePath}/{thumnailName}{Path.GetExtension(request.Thumbnail.FileName)}";
                await _transactionService.BeginTransactionAsync();
                await _repository.UpdateAsync(updatedCity);
                _imageService.UploadFile(request.Thumbnail, $"{storePath}", thumnailName); //upload the new file to server
                _imageService.DeleteFile(city.Thumbnail); // delete the old file from server
                await _transactionService.CommitTransactionAsync();
                return _mapper.Map<CityDto>(updatedCity);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception exception)
            {
                await _transactionService.RollbackTransactionAsync();
                if(updatedCity is not null)
                _imageService.DeleteFile(updatedCity.Thumbnail, true);
                throw new ErrorException($"Error during updaing city with id '{request.id}'.", exception);
            }
        }
    }
}
