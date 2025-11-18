using System.ComponentModel.DataAnnotations;

namespace Control_De_Tareas.Models
{
    public class UserVm
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

      public RolVm Rol { get; set; }
        public List<ModuleGroupVm> mewnu { get; set; }
    }
}