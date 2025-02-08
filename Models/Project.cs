using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CADProjectsHub.Models
{
    public class Project
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProjectID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProjectManager { get; set; }

        public ICollection<Assignment>? Assignments { get; set; }
    }
}
