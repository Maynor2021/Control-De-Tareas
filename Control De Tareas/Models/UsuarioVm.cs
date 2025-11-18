using System.ComponentModel.DataAnnotations;
namespace Control_De_Tareas.Models
{
    public class UsuarioVm
    {
        public Guid UserId { get; set; }
       

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres")]
        public string Password { get; set; }

        public RolVm Rol { get; set; }

        public List<ModuleGroupVm>Menu { get; set; }

    }
}
