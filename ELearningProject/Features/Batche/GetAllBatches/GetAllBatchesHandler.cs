
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.GetAllBatches
{
    public class GetAllBatchesHandler : IRequestHandler<GetAllBatchesQuery, EndpointResponse<PaginatedResult<BatchDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllBatchesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<PaginatedResult<BatchDto>>> Handle(GetAllBatchesQuery request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();

            // 1. Base Query
            var query = batchRepository.GetAll(trackChanges: false);

            // 2. Search Filter
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(b => b.Name.Contains(keyword));
            }
            
            // 3. Sorting (Default: Newest First)
            query = query.OrderByDescending(b => b.StartDate);

            // 4. Projection
            // Note: Counts might be expensive if many records. But for pagination it's okay for the page.
            // EF Core translates .Count() in projection efficiently usually.
            var projectedQuery = query.Select(b => new BatchDto(
                    b.Id,
                    b.Name,
                    b.StartDate,
                    b.Students.Count(bs => !bs.IsDeleted),
                    b.Tracks.Count));

            // 5. Total Count (Must be done on filtered query before paging)
            var totalCount = await query.CountAsync(cancellationToken);
            
            // 6. Pagination
            var items = await projectedQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<BatchDto>(items, totalCount, request.PageNumber, request.PageSize);

            return EndpointResponse<PaginatedResult<BatchDto>>.SuccessResponse(result);
        }
    }
}
