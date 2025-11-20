using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Tabla intermedia para relación muchos a muchos entre Rol y Permiso
    [Table("RolPermisos")]
    public class RolPermiso
    {
        [Key]
        public int RolPermisoId { get; set; }

        public int RolId { get; set; }
        [ForeignKey("RolId")]
        public virtual Rol Rol { get; set; }

        public int PermisoId { get; set; }
        [ForeignKey("PermisoId")]
        public virtual Permiso Permiso { get; set; }
    }
}
