namespace Control_De_Tareas.Models
{
    /// <summary>
    /// Representa un elemento del menú lateral del sistema.
    /// Contiene la información necesaria para renderizar la opción en la UI
    /// y determinar qué roles tienen acceso.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Identificador único del ítem del menú.
        /// Usado internamente para control y referencia.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Texto que se muestra al usuario en la interfaz.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Nombre del ícono que se mostrará en el menú (Lucide Icons).
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Lista de roles que tienen permiso para ver y acceder a este menú.
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// Nombre del controlador al que redirige el ítem del menú.
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Nombre de la acción del controlador a la que será redirigido el usuario.
        /// </summary>
        public string Action { get; set; }
    }
}
