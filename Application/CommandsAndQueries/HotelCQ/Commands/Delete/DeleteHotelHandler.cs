﻿using Application.Dtos.HotelDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Delete
{
    public class DeleteHotelHandler : IRequestHandler<DeleteHotelCommand, HotelMinDto>
    {
        private readonly IMapper _mapper;
        private readonly IHotelRepository _repository;
        private readonly IImageService _imageRepository;

        public DeleteHotelHandler(IHotelRepository repository, IImageService imageRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Hotel, HotelMinDto>();
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _imageRepository = imageRepository;
        }

        public async Task<HotelMinDto> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
        {
            Hotel deletedHotel;
            try
            {
                var hotel = await _repository.GetByIdAsync(request.Id) ?? throw new NotFoundException();
                await _repository.BeginTransactionAsync();
                deletedHotel = await _repository.DeleteAsync(hotel);
                _imageRepository.DeleteFile(hotel.Thumbnail);
                foreach (var image in hotel.Images)
                {
                    _imageRepository.DeleteFile(image.Path);
                }
                await _repository.CommitTransactionAsync();
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                await _repository.RollbackTransactionAsync();
                throw new ErrorException($"Error during deleting hotel with id '{request.Id}'.", exception);
            }
            return _mapper.Map<HotelMinDto>(deletedHotel);
        }
    }
}
