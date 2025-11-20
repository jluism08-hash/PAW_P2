using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Modelo de Inscripción de estudiantes en cursos
    [Table("Inscripciones")]
    public class Inscripcion
    {
        [Key]
        public int InscripcionId { get; set; }

        public int EstudianteId { get; set; }
        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }

        [Display(Name = "Fecha de Inscripción")]
        public DateTime FechaInscripcion { get; set; }

        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } // Activo, Retirado, Completado

        [Display(Name = "Nota Final")]
        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100")]
        public decimal? NotaFinal { get; set; }

        [Display(Name = "Aprobado")]
        public bool? Aprobado { get; set; }

        [Display(Name = "Fecha de Finalización")]
        public DateTime? FechaFinalizacion { get; set; }

        public Inscripcion()
        {
            FechaInscripcion = DateTime.Now;
            Estado = "Activo";
        }
    }
}
