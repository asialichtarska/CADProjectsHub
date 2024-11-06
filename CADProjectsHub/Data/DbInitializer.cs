using CADProjectsHub.Models;
using System;
using System.Linq;

namespace CADProjectsHub.Data
{
    public static class DbInitializer
    {
        public static void Initialize(CADProjectsContext context)
        {
            context.Database.EnsureCreated();

            // Sprawdź, czy istnieją już projekty, aby nie inicjalizować ponownie
            if (context.Projects.Any())
            {
                return;   // Baza danych jest już zainicjalizowana
            }

            var projects = new Project[]
            {
                new Project { ProjectID = 1, Name = "Soil drilling module", Description = "A design for taking a soil sample using an auger connected to a planetary gearbox that allows changing the direction of rotation.", ProjectManager = "Artur Bary" },
                new Project { ProjectID = 2, Name = "Mechanical reducer", Description = "A transmission in which the driven member has a lower speed than the driving member.", ProjectManager = "Maria Key" },
                new Project { ProjectID = 3, Name = "Robotic manipulator", Description = "A device used to manipulate components without direct physical contact by the operator.", ProjectManager = "Bob Piterson" },
            };

            foreach (Project p in projects)
            {
                context.Projects.Add(p);
            }
            context.SaveChanges();

            var cadModels = new CADModel[]
            {
                new CADModel { Name = "Drill", FileType = ".step", Manufacturing="lathing", ConstructorName = "Ethan Zak", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Leading bar", FileType = ".step", Manufacturing="lathing", ConstructorName = "Sophia Kimber", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Leading base", FileType = ".step", Manufacturing="milling", ConstructorName = "Sophia Kimber", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Back case", FileType = ".step", Manufacturing="lathing", ConstructorName = "Alex Salmon", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Bearing bar", FileType = ".step", Manufacturing="lathing", ConstructorName = "Sam Black", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Microscope clamp", FileType = ".stl", Manufacturing="3D printing", ConstructorName = "Alex Salmon", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Gear to holder", FileType = ".stl", Manufacturing="3D printing", ConstructorName = "Alex Salmon", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Internal gear wheel", FileType = ".step", Manufacturing="Rotary milling", ConstructorName = "Sam Black", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Satellite gear wheel", FileType = ".step", Manufacturing="Rotary milling", ConstructorName = "Sam Black", AssignmentDate = DateTime.Now },
                new CADModel { Name = "External gear wheel", FileType = ".step", Manufacturing="Rotary milling", ConstructorName = "Sam Black", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Machine shaft", FileType = ".step", Manufacturing="lathing", ConstructorName = "Tom Piterson", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Minor gear wheel", FileType = ".step", Manufacturing="Rotary milling", ConstructorName = "Eleonora Kraft", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Greater gear wheel", FileType = ".step", Manufacturing="Rotary milling", ConstructorName = "Eleonora Kraft", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Cover upper", FileType = ".step", Manufacturing="milling", ConstructorName = "Tom Piterson", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Cover bottom", FileType = ".step", Manufacturing="milling", ConstructorName = "Tom Piterson", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Gripper", FileType = ".stl", Manufacturing="3D printing", ConstructorName = "Nina Glass", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Main arm", FileType = ".step", Manufacturing="lathing", ConstructorName = "Tom Zayin", AssignmentDate = DateTime.Now },
                new CADModel { Name = "Middle arm", FileType = ".step", Manufacturing="lathing", ConstructorName = "Tom Zayin", AssignmentDate = DateTime.Now },
                new CADModel { Name = "End arm", FileType = ".step", Manufacturing="lathing", ConstructorName = "Tom Zayin", AssignmentDate = DateTime.Now },

            };

            foreach (CADModel cm in cadModels)
            {
                context.CADModels.Add(cm);
            }
            context.SaveChanges();

            var assignments = new Assignment[]
            {
                new Assignment { ProjectID = 1, CADModelID = 1 },
                new Assignment { ProjectID = 1, CADModelID = 2 },
                new Assignment { ProjectID = 1, CADModelID = 3 },
                new Assignment { ProjectID = 1, CADModelID = 4 },
                new Assignment { ProjectID = 1, CADModelID = 5 },
                new Assignment { ProjectID = 1, CADModelID = 6 },
                new Assignment { ProjectID = 1, CADModelID = 7 },
                new Assignment { ProjectID = 1, CADModelID = 8 },
                new Assignment { ProjectID = 1, CADModelID = 9 },
                new Assignment { ProjectID = 1, CADModelID = 10 },
                new Assignment { ProjectID = 2, CADModelID = 11 },
                new Assignment { ProjectID = 2, CADModelID = 12 },
                new Assignment { ProjectID = 2, CADModelID = 13 },
                new Assignment { ProjectID = 2, CADModelID = 14 },
                new Assignment { ProjectID = 2, CADModelID = 15 },
                new Assignment { ProjectID = 2, CADModelID = 16 },
                new Assignment { ProjectID = 2, CADModelID = 17 },
                new Assignment { ProjectID = 2, CADModelID = 18 },
                new Assignment { ProjectID = 2, CADModelID = 19 },

            };

            foreach (Assignment a in assignments)
            {
                context.Assignments.Add(a);
            }
            context.SaveChanges();
        }
    }
}
