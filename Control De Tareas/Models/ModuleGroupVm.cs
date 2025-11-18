namespace Control_De_Tareas.Models
{
    public class ModuleGroupVm
    {
        public Guid GroupModuleId { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set; }

        public List<ModuloVm> Modulos { get; set; } 


    }
}
