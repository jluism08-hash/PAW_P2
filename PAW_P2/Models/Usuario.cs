using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Modelo de Usuario del sistema
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(256)]
        public string Contrasena { get; set; }

        [StringLength(20)]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        [Display(Name = "Último Acceso")]
        public DateTime? UltimoAcceso { get; set; }

        // Relación con Rol
        public int RolId { get; set; }
        [ForeignKey("RolId")]
        public virtual Rol Rol { get; set; }

        // Relaciones
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }
        public virtual ICollection<CursoDocente> CursosAsignados { get; set; }
        public virtual ICollection<Bitacora> Bitacoras { get; set; }

        public Usuario()
        {
            FechaCreacion = DateTime.Now;
            Activo = true;
            Inscripciones = new HashSet<Inscripcion>();
            CursosAsignados = new HashSet<CursoDocente>();
            Bitacoras = new HashSet<Bitacora>();
        }
    }
}
