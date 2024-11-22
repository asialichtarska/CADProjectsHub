using System;
using System.Collections.Generic;

namespace CADProjectsHub.Models
{
    public class CADModel
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? FileType { get; set; }
        public string? Manufacturing { get; set; }
        public string? ConstructorName { get; set; }
        public DateTime AssignmentDate { get; set; }

        public ICollection<Assignment>? Assignments { get; set; }
    }
}