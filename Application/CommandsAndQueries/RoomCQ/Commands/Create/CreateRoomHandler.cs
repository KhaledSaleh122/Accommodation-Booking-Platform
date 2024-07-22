using Application.Dtos.RoomDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.RoomCQ.Commands.Create
{
    public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, RoomDto>
    {
        private readonly IMapper _mapper;
        private readonly IHotelRepository _hotelRepository;
        private readonly IImageService _imageRepository;

        public CreateRoomHandler(IHotelRepository hotelRepository, IImageService imageRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Room, RoomDto>()
                  .ForMember(dest => dest.Images, opt =>
                    opt.MapFrom(
                      src => src.Images.Select(x => x.Path).ToList()
                    )
                  );
                cfg.CreateMap<CreateRoomCommand, Room>()
                  .ForMember(dest => dest.Images, opt => opt.Ignore());
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ??
              throw new ArgumentNullException(nameof(hotelRepository));
            _imageRepository = imageRepository ??
              throw new ArgumentNullException(nameof(imageRepository));
        }
        public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            var room = _mapper.Map<Room>(request);
            room.HotelId = request.hotelId;
            var storePath = "RoomImages";
            var thumbnailPath = "Thumbnails";
            var imagesPath = "Images";
            var thumnailName = Guid.NewGuid().ToString();
            List<string> imagesName = [];
            room.Thumbnail =
            $"{storePath}/{thumbnailPath}/{thumnailName}{Path.GetExtension(request.Thumbnail.FileName)}";
            room.Images = request.Images
              .Select(image =>
              {
                  var imageName = Guid.NewGuid().ToString();
                  imagesName.Add(imageName);
                  return new RoomImage()
                  {
                      Path = $"{storePath}/{imagesPath}/{imageName}{Path.GetExtension(image.FileName)}"
                  };
              }).ToList();
            try
            {
                var isRoomNumberExist = await _hotelRepository.RoomNumberExistsAsync(
                    request.hotelId,
                    request.RoomNumber
                );
                if (isRoomNumberExist)
                    throw new ErrorException("The RoomNumber already Exist.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var hotel = await _hotelRepository.GetByIdAsync(request.hotelId)
                    ?? throw new NotFoundException("Hotel not Found!");
                await _hotelRepository.BeginTransactionAsync();
                await _hotelRepository.AddRoomAsync(room);
                _imageRepository.UploadFile(request.Thumbnail, $"{storePath}\\{thumbnailPath}", thumnailName);
                for (int i = 0; i < request.Images.Count; i++)
                {
                    _imageRepository.UploadFile(request.Images[i], $"{storePath}\\{imagesPath}", imagesName[i]);
                }
                await _hotelRepository.CommitTransactionAsync();
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ErrorException) {
                throw;
            }
            catch (Exception exception)
            {

                await _hotelRepository.RollbackTransactionAsync();
                _imageRepository.DeleteFile(room.Thumbnail, true);
                foreach (var image in room.Images)
                {
                    _imageRepository.DeleteFile(image.Path);
                }
                throw new ErrorException($"Error during creating new Room.", exception);
            }
            var roomDto = _mapper.Map<RoomDto>(room);
            return roomDto;
        }
    }
}