using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Modelo de Permiso para control de acceso granular
    [Table("Permisos")]
    public class Permiso
    {
        [Key]
        public int PermisoId { get; set; }

        [Required(ErrorMessage = "El nombre del permiso es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre del Permiso")]
        public string Nombre { get; set; }

        [StringLength(200)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Módulo")]
        public string Modulo { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        // Relaciones
        public virtual ICollection<RolPermiso> RolPermisos { get; set; }

        public Permiso()
        {
            Activo = true;
            RolPermisos = new HashSet<RolPermiso>();
        }
    }
}
