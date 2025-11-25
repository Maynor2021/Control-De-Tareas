namespace Control_De_Tareas.Models
{
    public class TareaVm
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaEntrega { get; set; }
        public decimal MaxScore { get; set; }
    }
}

