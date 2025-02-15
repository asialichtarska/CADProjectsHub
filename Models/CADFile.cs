using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CADProjectsHub.Models
{
    public class CADFile
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty; // .step, .stl, .pdf

        [Required]
        public string FilePath { get; set; } = string.Empty; // Ścieżka do pliku na serwerze

        public int CADModelID { get; set; } // Powiązanie z modelem CAD
        public CADModel? CADModel { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}