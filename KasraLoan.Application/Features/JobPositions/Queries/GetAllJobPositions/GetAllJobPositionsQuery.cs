using MediatR;

namespace KasraLoan.Application.Features.JobPositions.Queries.GetAllJobPositions
{
    public class GetAllJobPositionsQuery : IRequest<GetAllJobPositionsResponse>
    {
        /// <summary>اگر true باشد فقط سمت‌های فعال برگردانده می‌شوند.</summary>
        public bool ActiveOnly { get; set; }
    }
}
