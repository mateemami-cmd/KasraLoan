using KasraLoan.Application.DTOs.LoanTypes;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanTypes.Queries.GetAllLoanTypes
{
    public class GetAllLoanTypesHandler
        : IRequestHandler<GetAllLoanTypesQuery, GetAllLoanTypesResponse>
    {
        private readonly ILoanTypeRepository _loanTypeRepository;

        public GetAllLoanTypesHandler(ILoanTypeRepository loanTypeRepository)
        {
            _loanTypeRepository = loanTypeRepository;
        }

        public async Task<GetAllLoanTypesResponse> Handle(
            GetAllLoanTypesQuery request,
            CancellationToken cancellationToken)
        {
            var loanTypes = await _loanTypeRepository.GetAllAsync();

            if (request.ActiveOnly)
            {
                loanTypes = loanTypes.Where(x => x.IsActive).ToList();
            }

            var items = loanTypes.Select(x => new LoanTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type.ToString(),
                IsActive = x.IsActive
            })
                .ToList();

            return new GetAllLoanTypesResponse
            {
                Items = items
            };
        }
    }
}
