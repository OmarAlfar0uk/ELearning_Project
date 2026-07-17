using ELearningProject.Models;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Lightweight view of a single coin transaction for API responses.
    /// </summary>
    /// <param name="Id">Transaction identifier.</param>
    /// <param name="Amount">Number of coins credited.</param>
    /// <param name="Reason">Human-readable reason for the grant.</param>
    /// <param name="Source">The event that triggered the transaction.</param>
    /// <param name="GrantedByAdminName">Full name of the admin/instructor who issued the grant, or <c>null</c> for system-generated transactions.</param>
    /// <param name="CreatedAt">UTC timestamp of the transaction.</param>
    public record CoinTransactionDto(
        Guid Id,
        int Amount,
        string Reason,
        CoinSource Source,
        string? GrantedByAdminName,
        DateTime CreatedAt);

    /// <summary>
    /// A student's current coin balance plus their full transaction history (paginated, newest first).
    /// </summary>
    /// <param name="StudentId">The student's identifier.</param>
    /// <param name="StudentName">The student's full name.</param>
    /// <param name="CoinBalance">Current running coin balance.</param>
    /// <param name="History">Paginated list of transactions ordered newest-first.</param>
    public record StudentCoinsDto(
        Guid StudentId,
        string StudentName,
        int CoinBalance,
        List<CoinTransactionDto> History);

    /// <summary>
    /// Summary of a single student's coin standing for the leaderboard view.
    /// </summary>
    /// <param name="StudentId">The student's identifier.</param>
    /// <param name="StudentName">The student's full name.</param>
    /// <param name="CoinBalance">The student's current coin balance.</param>
    public record CoinTopStudentDto(
        Guid StudentId,
        string StudentName,
        int CoinBalance);

    /// <summary>
    /// System-wide coins distribution snapshot for Admin/Instructor dashboards.
    /// </summary>
    /// <param name="TotalDistributed">Total coins ever distributed across all students.</param>
    /// <param name="TopStudents">Top 10 students ordered by descending <see cref="CoinBalance"/>.</param>
    /// <param name="RecentTransactions">The 20 most recent transactions across all students.</param>
    public record CoinsDistributionDto(
        int TotalDistributed,
        List<CoinTopStudentDto> TopStudents,
        List<CoinTransactionDto> RecentTransactions);
}
