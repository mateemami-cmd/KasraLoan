using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Queries.GetNextIdentifier
{
    public class GetNextIdentifierHandler
        : IRequestHandler<GetNextIdentifierQuery, GetNextIdentifierResponse>
    {
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IUsernameGenerator _usernameGenerator;

        public GetNextIdentifierHandler(
            IJobPositionRepository jobPositionRepository,
            IUsernameGenerator usernameGenerator)
        {
            _jobPositionRepository = jobPositionRepository;
            _usernameGenerator = usernameGenerator;
        }

        public async Task<GetNextIdentifierResponse> Handle(
            GetNextIdentifierQuery request,
            CancellationToken cancellationToken)
        {
            var position = await _jobPositionRepository.GetByIdAsync(request.JobPositionId);

            if (position == null)
                throw new BusinessRuleException("سمت شغلی انتخاب‌شده یافت نشد.");

            var identifier = await _usernameGenerator.GenerateAsync(request.HireDate, position);

            return new GetNextIdentifierResponse { Identifier = identifier };
        }
    }
}
