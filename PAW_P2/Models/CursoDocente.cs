using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Tabla intermedia para asignación de docentes a cursos
    [Table("CursoDocentes")]
    public class CursoDocente
    {
        [Key]
        public int CursoDocenteId { get; set; }

        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }

        public int DocenteId { get; set; }
        [ForeignKey("DocenteId")]
        public virtual Usuario Docente { get; set; }

        [Display(Name = "Fecha de Asignación")]
        public DateTime FechaAsignacion { get; set; }

        [StringLength(20)]
        [Display(Name = "Horario")]
        public string Horario { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        public CursoDocente()
        {
            FechaAsignacion = DateTime.Now;
            Activo = true;
        }
    }
}
