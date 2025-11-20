using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAW_P2.Models
{
    // Modelo de Bitácora para auditoría del sistema
    [Table("Bitacoras")]
    public class Bitacora
    {
        [Key]
        public int BitacoraId { get; set; }

        public int? UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Acción")]
        public string Accion { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Módulo")]
        public string Modulo { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        [StringLength(50)]
        [Display(Name = "Dirección IP")]
        public string DireccionIP { get; set; }

        [StringLength(200)]
        [Display(Name = "Navegador")]
        public string Navegador { get; set; }

        [StringLength(100)]
        [Display(Name = "Entidad Afectada")]
        public string EntidadAfectada { get; set; }

        public int? EntidadId { get; set; }

        [StringLength(1000)]
        [Display(Name = "Datos Anteriores")]
        public string DatosAnteriores { get; set; }

        [StringLength(1000)]
        [Display(Name = "Datos Nuevos")]
        public string DatosNuevos { get; set; }

        public Bitacora()
        {
            FechaHora = DateTime.Now;
        }
    }
}
