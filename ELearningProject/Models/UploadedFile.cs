using Auth.Models;

namespace ELearningProject.Models
{
    public class UploadedFile : BaseEntity
    {
        public string FileName { get; set; } = string.Empty; // User's original file name
        public string StoredName { get; set; } = string.Empty; // GUID based name on disk
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Folder { get; set; } = "General";
        
        // Optional: Link to the user who uploaded it
        public Guid? UploadedById { get; set; }
        public ApplicationUser? UploadedBy { get; set; }
    }
}
