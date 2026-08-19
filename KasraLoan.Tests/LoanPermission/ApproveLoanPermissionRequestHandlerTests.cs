using FluentAssertions;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Features.LoanPermission.Commands.ApproveLoanPermissionRequest;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using Xunit;

namespace KasraLoan.Tests.LoanPermission;

public class ApproveLoanPermissionRequestHandlerTests
{
    private readonly Mock<ILoanPermissionRequestRepository> _permissionRepository;
    private readonly Mock<IEmployeeScoreRepository> _employeeScoreRepository;
    private readonly Mock<INotificationService> _notificationService;
    private readonly Mock<ICurrentUserService> _currentUserService;

    private readonly ApproveLoanPermissionRequestHandler _handler;

    public ApproveLoanPermissionRequestHandlerTests()
    {
        _permissionRepository = new Mock<ILoanPermissionRequestRepository>();
        _employeeScoreRepository = new Mock<IEmployeeScoreRepository>();
        _notificationService = new Mock<INotificationService>();
        _currentUserService = new Mock<ICurrentUserService>();

        // ادمین ارشد فرض می‌شود؛ به همه‌ی انواع وام دسترسی دارد.
        _currentUserService.Setup(x => x.CanManageLoanType(It.IsAny<int>())).Returns(true);

        _handler = new ApproveLoanPermissionRequestHandler(
            _permissionRepository.Object,
            _employeeScoreRepository.Object,
            _notificationService.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_Should_Approve_And_Activate_OneTime_Permission()
    {
        var employeeId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var permissionRequest = new Domain.Entities.LoanPermissionRequest
        {
            Id = requestId,
            EmployeeId = employeeId,
            LoanTypeId = 4,
            Status = LoanPermissionRequestStatus.Pending,
            LoanType = new LoanType { Id = 4, Name = "وام ازدواج" }
        };

        var scoreRecord = new EmployeeScore { EmployeeId = employeeId };

        _permissionRepository
            .Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(permissionRequest);

        _employeeScoreRepository
            .Setup(x => x.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(scoreRecord);

        var result = await _handler.Handle(
            new ApproveLoanPermissionRequestCommand { PermissionRequestId = requestId },
            CancellationToken.None);

        result.Should().NotBeNull();

        permissionRequest.Status.Should().Be(LoanPermissionRequestStatus.Approved);
        permissionRequest.ReviewedAt.Should().NotBeNull();

        scoreRecord.HasLoanPermissionOverride.Should().BeTrue();
        scoreRecord.PermissionGrantedAt.Should().NotBeNull();

        _permissionRepository.Verify(x => x.SaveChangesAsync(), Times.Once);

        _notificationService.Verify(
            x => x.SendAsync(employeeId, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Request_Not_Found()
    {
        var requestId = Guid.NewGuid();

        _permissionRepository
            .Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync((Domain.Entities.LoanPermissionRequest?)null);

        Func<Task> action = async () =>
            await _handler.Handle(
                new ApproveLoanPermissionRequestCommand { PermissionRequestId = requestId },
                CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Request_Already_Reviewed()
    {
        var requestId = Guid.NewGuid();

        var permissionRequest = new Domain.Entities.LoanPermissionRequest
        {
            Id = requestId,
            EmployeeId = Guid.NewGuid(),
            Status = LoanPermissionRequestStatus.Approved
        };

        _permissionRepository
            .Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(permissionRequest);

        Func<Task> action = async () =>
            await _handler.Handle(
                new ApproveLoanPermissionRequestCommand { PermissionRequestId = requestId },
                CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();

        _permissionRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}
