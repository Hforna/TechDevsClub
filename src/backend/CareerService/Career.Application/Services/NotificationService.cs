using AutoMapper;
using Career.Application.Responses;
using Career.Domain.Entities;
using Career.Domain.Exceptions;
using Career.Domain.Repositories;
using Career.Domain.Services.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Services
{
    public interface INotificationService
    {
        public Task<NotificationsPaginatedResponse> GetUserNotificationsPaginated(int page, int perPage);
        public Task<NotificationResponse> UserNotificationById(Guid id);
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IProfileServiceClient _profileService;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private readonly string _accessToken;

        public NotificationService(ILogger<NotificationService> logger, IUnitOfWork uow, 
            IProfileServiceClient profileService, IRequestService requestService, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
            _profileService = profileService;
            _requestService = requestService;

            _accessToken = _requestService.GetBearerToken()!;
        }

        public async Task<NotificationsPaginatedResponse> GetUserNotificationsPaginated(int page, int perPage)
        {
            var user = await _profileService.GetUserInfos(_accessToken);

            var notifications = _uow.NotificationRepository.GetNotificatonsPaginated(perPage, page, user.id);

            var response = new NotificationsPaginatedResponse()
            {
                HasNextPage = notifications.HasNextPage,
                HasPreviousPage = notifications.HasPreviousPage,
                IsFirstPage = notifications.IsFirstPage,
                IsLastPage = notifications.IsLastPage,
                Count = notifications.Count,
                Notifications = notifications
                .Select(notification => _mapper.Map<ShortNotificationResponse>(notification))
                .ToList()
            };

            return response;
        }

        public async Task<NotificationResponse> UserNotificationById(Guid id)
        {
            var user = await _profileService.GetUserInfos(_accessToken);

            var notification = await _uow.GenericRepository.GetById<Notification>(id, true) 
                ?? throw new NullEntityException(ResourceExceptMessages.NOTIFICATION_NOT_EXISTS);

            if (notification.UserId != user.id)
                throw new DomainException(ResourceExceptMessages.NOTIFICATION_NOT_OF_USER);

            notification.IsRead = true;

            _uow.GenericRepository.Update<Notification>(notification);
            await _uow.Commit();

            return _mapper.Map<NotificationResponse>(notification);
        }
    }
}
