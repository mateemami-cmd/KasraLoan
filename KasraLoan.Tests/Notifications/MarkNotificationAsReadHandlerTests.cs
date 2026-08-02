using FluentAssertions;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using Moq;
using Xunit;

namespace KasraLoan.Tests.Notifications;

public class MarkNotificationAsReadHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;

    private readonly MarkNotificationAsReadHandler _handler;

    private readonly Guid _employeeId = Guid.NewGuid();

    public MarkNotificationAsReadHandlerTests()
    {
        _notificationRepository = new Mock<INotificationRepository>();
        _currentUserService = new Mock<ICurrentUserService>();

        _currentUserService.Setup(x => x.UserId).Returns(_employeeId);

        _handler = new MarkNotificationAsReadHandler(
            _notificationRepository.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_Should_Mark_Own_Notification_As_Read()
    {
        var notificationId = Guid.NewGuid();

        var notification = new Notification
        {
            Id = notificationId,
            EmployeeId = _employeeId,
            IsRead = false
        };

        _notificationRepository
            .Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        await _handler.Handle(
            new MarkNotificationAsReadCommand { NotificationId = notificationId },
            CancellationToken.None);

        notification.IsRead.Should().BeTrue();

        _notificationRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Forbidden_When_Notification_Belongs_To_Another_User()
    {
        var notificationId = Guid.NewGuid();

        var notification = new Notification
        {
            Id = notificationId,
            EmployeeId = Guid.NewGuid(), // متعلق به کارمند دیگری
            IsRead = false
        };

        _notificationRepository
            .Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        Func<Task> action = async () =>
            await _handler.Handle(
                new MarkNotificationAsReadCommand { NotificationId = notificationId },
                CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenAccessException>();

        _notificationRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Notification_Not_Found()
    {
        var notificationId = Guid.NewGuid();

        _notificationRepository
            .Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        Func<Task> action = async () =>
            await _handler.Handle(
                new MarkNotificationAsReadCommand { NotificationId = notificationId },
                CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }
}
