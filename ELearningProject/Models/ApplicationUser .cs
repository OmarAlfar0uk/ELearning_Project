using ELearningProject.Contracts;
using ELearningProject.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Models
{
    public class ApplicationUser : IdentityUser<Guid>, IBaseEntity
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }

        public bool IsActivated { get; set; } = false;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? LastLoginAt { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        public ICollection<InstructorTrack> InstructorTracks { get; set; } = new HashSet<InstructorTrack>();

        /// <summary>
        /// Running total of coins credited to this user.
        /// Always kept consistent with the sum of <see cref="CoinTransactions"/> via atomic writes.
        /// </summary>
        public int CoinBalance { get; set; } = 0;

        /// <summary>Navigation collection of all coin transactions for this student.</summary>
        public ICollection<ELearningProject.Models.CoinTransaction> CoinTransactions { get; set; } = new HashSet<ELearningProject.Models.CoinTransaction>();
    }

}
