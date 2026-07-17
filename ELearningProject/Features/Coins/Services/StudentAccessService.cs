using ELearningProject.Contarcts;
using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Coins.Services
{
    /// <summary>
    /// Determines whether a caller is permitted to act on a specific student's coin data.
    /// </summary>
    public interface IStudentAccessService
    {
        /// <summary>
        /// Returns <c>true</c> when the caller is allowed to read or write coin data for
        /// <paramref name="studentId"/>; <c>false</c> when access must be denied (403).
        /// </summary>
        /// <remarks>
        /// Decision matrix:
        /// <list type="bullet">
        ///   <item>Role <c>Admin</c> or <c>SuperAdmin</c> → always <c>true</c>, no DB hit.</item>
        ///   <item>Role <c>Student</c> → always <c>false</c> from this helper
        ///     (student self-only logic is handled upstream in the handlers).</item>
        ///   <item>Role <c>Instructor</c> → runs the track-scoped join query:
        ///     the instructor must own at least one <see cref="InstructorTrack"/> whose
        ///     <c>Track.BatchId</c> matches a <see cref="BatchStudent"/> row for
        ///     <paramref name="studentId"/>.</item>
        /// </list>
        /// </remarks>
        /// <param name="callerRole">The role claim of the authenticated caller.</param>
        /// <param name="instructorId">The caller's user ID (used only when role is <c>Instructor</c>).</param>
        /// <param name="studentId">The target student whose coin data is being accessed.</param>
        /// <param name="cancellationToken">Propagated cancellation token.</param>
        Task<bool> CanAccessStudentCoinsAsync(
            string callerRole,
            Guid instructorId,
            Guid studentId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of <see cref="IStudentAccessService"/>.
    /// </summary>
    public class StudentAccessService : IStudentAccessService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentAccessService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public async Task<bool> CanAccessStudentCoinsAsync(
            string callerRole,
            Guid instructorId,
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            // ── Admin / SuperAdmin: unconditional access, skip DB query ───────────
            if (callerRole is "Admin" or "SuperAdmin")
            {
                return true;
            }

            // ── Instructor: must share at least one batch with the student ─────────
            // Join path:
            //   InstructorTracks (InstructorId == instructorId)
            //     → Track (Id == InstructorTracks.TrackId)
            //     → BatchStudents (BatchId == Track.BatchId AND StudentId == studentId)
            //
            // The query is deliberately a single EXISTS-style AnyAsync so the DB
            // engine can short-circuit after finding the first matching row.
            if (callerRole == "Instructor")
            {
                var instructorTrackRepo = _unitOfWork.GetRepository<InstructorTrack>();
                var batchStudentRepo    = _unitOfWork.GetRepository<BatchStudent>();

                // Collect the set of BatchIds this instructor is responsible for.
                // Using a subquery projection so EF translates the whole thing to SQL.
                var instructorBatchIds = instructorTrackRepo
                    .FindByCondition(it => it.InstructorId == instructorId)
                    .Select(it => it.Track.BatchId);  // EF translates to JOIN on Track

                // Check whether the target student is enrolled in any of those batches.
                var hasAccess = await batchStudentRepo
                    .FindByCondition(bs => bs.StudentId == studentId)
                    .AnyAsync(bs => instructorBatchIds.Contains(bs.BatchId), cancellationToken);

                return hasAccess;
            }

            // ── Any other role (Student, etc.) → deny; caller handles self-only ───
            return false;
        }
    }
}
