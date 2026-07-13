using ELearningProject.Contarcts; // For IUnitOfWork and IGenericRepository (note typo in namespace)
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ELearningProject.Features.Materials
{
    /// <summary>
    /// Command to soft-delete a material.
    /// </summary>
    public record DeleteMaterialCommand(Guid MaterialId) : IRequest<EndpointResponse<string>>;

    /// <summary>
    /// Handler to process DeleteMaterialCommand.
    /// </summary>
    public class DeleteMaterialHandler : IRequestHandler<DeleteMaterialCommand, EndpointResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMaterialHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<string>> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
        {
            var materialRepository = _unitOfWork.GetRepository<Material>();
            var material = await materialRepository.GetByIdAsync(request.MaterialId);
            if (material == null)
            {
                return EndpointResponse<string>.NotFoundResponse("Material not found.");
            }

            // Perform Soft-delete
            material.IsDeleted = true;
            material.UpdatedAt = DateTime.UtcNow;

            materialRepository.Update(material);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<string>.SuccessResponse("Material deleted successfully.", "Material deleted successfully.");
        }
    }
}
