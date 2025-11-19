using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Mapster;

namespace Control_De_Tareas
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            // Configuración para CourseOfferings -> CursoDto
            TypeAdapterConfig<CourseOfferings, CursoDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Codigo, src => src.Course.Code)
                .Map(dest => dest.Nombre, src => src.Course.Title)
                .Map(dest => dest.InstructorNombre, src => src.Professor.FullName ?? src.Professor.UserName)
                .Map(dest => dest.CantidadEstudiantes, src => src.Enrollments.Count(e => !e.IsSoftDeleted && e.Status == "Active"))
                .Map(dest => dest.Estado, src => src.IsActive ? "Activo" : "Inactivo");
        }
    }
}
