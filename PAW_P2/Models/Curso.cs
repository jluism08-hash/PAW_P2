using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Modelo de Curso académico
    [Table("Cursos")]
    public class Curso
    {
        [Key]
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El código del curso es requerido")]
        [StringLength(20)]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre del curso es requerido")]
        [StringLength(150)]
        [Display(Name = "Nombre del Curso")]
        public string Nombre { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Los créditos son requeridos")]
        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        [Display(Name = "Créditos")]
        public int Creditos { get; set; }

        [Required(ErrorMessage = "El cuatrimestre es requerido")]
        [StringLength(50)]
        [Display(Name = "Cuatrimestre")]
        public string Cuatrimestre { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; }

        // Usuario que creó el curso
        public int? CreadoPorId { get; set; }

        [Display(Name = "Fecha de Modificación")]
        public DateTime? FechaModificacion { get; set; }

        // Usuario que modificó el curso
        public int? ModificadoPorId { get; set; }

        // Relaciones
        public virtual ICollection<CursoDocente> DocentesAsignados { get; set; }
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }
        public virtual ICollection<Calificacion> Calificaciones { get; set; }

        public Curso()
        {
            Activo = true;
            FechaCreacion = DateTime.Now;
            DocentesAsignados = new HashSet<CursoDocente>();
            Inscripciones = new HashSet<Inscripcion>();
            Calificaciones = new HashSet<Calificacion>();
        }
    }
}
