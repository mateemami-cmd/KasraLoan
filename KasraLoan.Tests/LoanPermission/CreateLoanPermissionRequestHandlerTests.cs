using FluentAssertions;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.LoanPermission;
using KasraLoan.Application.Features.LoanPermission.Commands.CreateLoanPermissionRequest;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using Xunit;

namespace KasraLoan.Tests.LoanPermission;

public class CreateLoanPermissionRequestHandlerTests
{
    private readonly Mock<ILoanPermissionRequestRepository> _permissionRepository;
    private readonly Mock<ILoanTypeRepository> _loanTypeRepository;
    private readonly Mock<IEmployeeRepository> _employeeRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<INotificationService> _notificationService;

    private readonly CreateLoanPermissionRequestHandler _handler;

    private readonly Guid _employeeId = Guid.NewGuid();

    public CreateLoanPermissionRequestHandlerTests()
    {
        _permissionRepository = new Mock<ILoanPermissionRequestRepository>();
        _loanTypeRepository = new Mock<ILoanTypeRepository>();
        _employeeRepository = new Mock<IEmployeeRepository>();
        _currentUserService = new Mock<ICurrentUserService>();
        _notificationService = new Mock<INotificationService>();

        _currentUserService.Setup(x => x.UserId).Returns(_employeeId);

        _handler = new CreateLoanPermissionRequestHandler(
            _permissionRepository.Object,
            _loanTypeRepository.Object,
            _employeeRepository.Object,
            _currentUserService.Object,
            _notificationService.Object);
    }

    private CreateLoanPermissionRequestCommand BuildCommand() => new()
    {
        Request = new CreateLoanPermissionRequestDto
        {
            LoanTypeId = 4,
            Reason = "به دلیل هزینه‌های ازدواج به این وام نیاز دارم."
        }
    };

    [Fact]
    public async Task Handle_Should_Create_Request_When_Valid()
    {
        _employeeRepository
            .Setup(x => x.GetByIdAsync(_employeeId))
            .ReturnsAsync(new Employee { Id = _employeeId });

        _loanTypeRepository
            .Setup(x => x.GetByIdAsync(4))
            .ReturnsAsync(new LoanType { Id = 4, Name = "وام ازدواج", IsActive = true });

        _permissionRepository
            .Setup(x => x.HasPendingRequestAsync(_employeeId))
            .ReturnsAsync(false);

        var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.RequestId.Should().NotBeEmpty();

        _permissionRepository.Verify(
            x => x.AddAsync(It.Is<Domain.Entities.LoanPermissionRequest>(r =>
                r.EmployeeId == _employeeId &&
                r.LoanTypeId == 4 &&
                r.Status == LoanPermissionRequestStatus.Pending)),
            Times.Once);

        _permissionRepository.Verify(x => x.SaveChangesAsync(), Times.Once);

        _notificationService.Verify(
            x => x.SendAsync(_employeeId, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_LoanType_Is_Inactive()
    {
        _employeeRepository
            .Setup(x => x.GetByIdAsync(_employeeId))
            .ReturnsAsync(new Employee { Id = _employeeId });

        _loanTypeRepository
            .Setup(x => x.GetByIdAsync(4))
            .ReturnsAsync(new LoanType { Id = 4, Name = "وام ازدواج", IsActive = false });

        Func<Task> action = async () =>
            await _handler.Handle(BuildCommand(), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();

        _permissionRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.LoanPermissionRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_A_Pending_Request_Already_Exists()
    {
        _employeeRepository
            .Setup(x => x.GetByIdAsync(_employeeId))
            .ReturnsAsync(new Employee { Id = _employeeId });

        _loanTypeRepository
            .Setup(x => x.GetByIdAsync(4))
            .ReturnsAsync(new LoanType { Id = 4, Name = "وام ازدواج", IsActive = true });

        _permissionRepository
            .Setup(x => x.HasPendingRequestAsync(_employeeId))
            .ReturnsAsync(true);

        Func<Task> action = async () =>
            await _handler.Handle(BuildCommand(), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();

        _permissionRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.LoanPermissionRequest>()),
            Times.Never);
    }
}
