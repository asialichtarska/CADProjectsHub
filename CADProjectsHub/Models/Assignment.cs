using System.Diagnostics;

namespace CADProjectsHub.Models
{
    public class Assignment
    {
        public int AssignmentID { get; set; }
        public int ProjectID { get; set; }
        public int CADModelID { get; set; }

        public Project? Project { get; set; }
        public CADModel? CADModel { get; set; }
    }
}
