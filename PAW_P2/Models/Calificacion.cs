using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Modelo de Calificación para evaluaciones
    [Table("Calificaciones")]
    public class Calificacion
    {
        [Key]
        public int CalificacionId { get; set; }

        public int EstudianteId { get; set; }
        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }

        [Required(ErrorMessage = "El tipo de evaluación es requerido")]
        [StringLength(50)]
        [Display(Name = "Tipo de Evaluación")]
        public string TipoEvaluacion { get; set; } // Tarea, Examen, Participación, Proyecto

        [StringLength(200)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La nota es requerida")]
        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [Display(Name = "Nota")]
        public decimal Nota { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        [Display(Name = "Porcentaje")]
        public decimal Porcentaje { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }

        public Calificacion()
        {
            FechaRegistro = DateTime.Now;
        }
    }
}
