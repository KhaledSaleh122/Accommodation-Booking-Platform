﻿using Application.Dtos.HotelDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Create
{
    internal class CreateHotelHandler : IRequestHandler<CreateHotelCommand, HotelMinDto>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IImageService _imageRepository;
        private readonly IMapper _mapper;
        private readonly ITransactionService _transactionService;

        public CreateHotelHandler(
            IHotelRepository hotelRepository,
            IImageService imageRepository,
            ICityRepository cityRepository,
            ITransactionService transactionService)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Hotel, HotelMinDto>()
                   .ForMember(dest => dest.City,
                       opt =>
                            opt.MapFrom(src => src.City.Name)
                   )
                   .ForMember(dest => dest.Country,
                       opt =>
                            opt.MapFrom(src => src.City.Country)
                   )
                   .ForMember(dest => dest.Images,
                       opt =>
                            opt.MapFrom(src => src.Images.Select(x => x.Path).ToList())
                   );
                cfg.CreateMap<CreateHotelCommand, Hotel>()
                   .ForMember(dest => dest.Images, opt => opt.Ignore())
                   .ForMember(m => m.HotelType, opt =>
                   {
                       opt.MapFrom(src => (HotelType)src.HotelType!);
                   });
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        public async Task<HotelMinDto> Handle(
                CreateHotelCommand request,
                CancellationToken cancellationToken
            )
        {
            var hotel = _mapper.Map<Hotel>(request);
            var storePath = "HotelImages";
            var thumbnailPath = "Thumbnails";
            var imagesPath = "Images";
            var thumnailName = Guid.NewGuid().ToString();
            List<string> imagesName = [];
            hotel.CityId = request.CityId;
            hotel.Thumbnail = 
                $"{storePath}/{thumbnailPath}/{thumnailName}{Path.GetExtension(request.Thumbnail.FileName)}";
            hotel.Images = 
                request.Images
                .Select(image =>
                {
                    var imageName = Guid.NewGuid().ToString();
                    imagesName.Add(imageName);
                    return new HotelImage()
                    {
                        Path = $"{storePath}/{imagesPath}/{imageName}{Path.GetExtension(image.FileName)}"
                    };
                }).ToList();
            try
            {
                await _transactionService.BeginTransactionAsync();
                await _hotelRepository.CreateAsync(hotel);
                _imageRepository.UploadFile(request.Thumbnail, $"{storePath}\\{thumbnailPath}", thumnailName);

                for (int i = 0; i < request.Images.Count; i++)
                {
                    _imageRepository.UploadFile(request.Images[i], $"{storePath}\\{imagesPath}", imagesName[i]);
                }
                await _transactionService.CommitTransactionAsync();
            }
            catch (Exception exception)
            {
                await _transactionService.RollbackTransactionAsync();
                _imageRepository.DeleteFile(hotel.Thumbnail,true);
                    foreach (var image in hotel.Images)
                {
                    _imageRepository.DeleteFile(image.Path,true);
                }
                throw new ErrorException($"Error during creating new hotel.",exception);
            }
            var hotelDto = _mapper.Map<HotelMinDto>(hotel);
            return hotelDto;
        }
    }
}
