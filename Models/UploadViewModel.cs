using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CADProjectsHub.Models
{
    public class UploadViewModel
    {
        public int SelectedCADModelID { get; set; }
        public string? NewCADModelName { get; set; }
        public string Manufacturing { get; set; }
        public string ConstructorName { get; set; }
        public IFormFile File { get; set; } = null!;
        public List<CADModel> CADModels { get; set; } = new List<CADModel>();
    }
}