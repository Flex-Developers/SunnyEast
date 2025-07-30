using Application.Contract.NotificationSubscriptions;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Notifications;

public class NotificationsMapperProfile : Profile
{
    public NotificationsMapperProfile()
    {
        CreateMap<CreateNotificationSubscriptionCommand, NotificationSubscription>()
            .ForMember(opt => opt.P256Dh, ex => ex.MapFrom(src => src.Keys.P256dh))
            .ForMember(opt => opt.Auth, ex => ex.MapFrom(src => src.Keys.Auth));
    }
}