using CADProjectsHub.Crypto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CADProjectsHub.Models
{
    public class CADModel
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? FileType { get; set; }
        public string? Manufacturing { get; set; }
        public string? ConstructorName { get; set; }
        public string? ConstructorInitializationVector { get; set; }
        public DateTime AssignmentDate { get; set; }


        [NotMapped]
        public string? ConstructorNameEncrypted { get; set; }

        public ICollection<Assignment>? Assignments { get; set; }
    }
}