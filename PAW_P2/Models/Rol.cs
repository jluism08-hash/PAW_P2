using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Modelo de Rol para control de acceso
    [Table("Roles")]
    public class Rol
    {
        [Key]
        public int RolId { get; set; }

        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(50)]
        [Display(Name = "Nombre del Rol")]
        public string Nombre { get; set; }

        [StringLength(200)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; }

        // Relaciones
        public virtual ICollection<Usuario> Usuarios { get; set; }
        public virtual ICollection<RolPermiso> RolPermisos { get; set; }

        public Rol()
        {
            Activo = true;
            FechaCreacion = DateTime.Now;
            Usuarios = new HashSet<Usuario>();
            RolPermisos = new HashSet<RolPermiso>();
        }
    }
}
