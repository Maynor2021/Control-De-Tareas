<<<<<<< HEAD
﻿namespace Control_De_Tareas.Models
=======
﻿using System.ComponentModel.DataAnnotations;

namespace Control_De_Tareas.Models
>>>>>>> 33251b12da292b3cd7aa9f4be08621805c2e0e30
{
    public class RolVm
    {
        public Guid RoleId { get; set; }
<<<<<<< HEAD
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
      

    }
}
=======
        public string RoleName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public DateTime CreateAt { get; set; }
        public bool IsSoftDeleted { get; set; }
    }
}
>>>>>>> 33251b12da292b3cd7aa9f4be08621805c2e0e30
