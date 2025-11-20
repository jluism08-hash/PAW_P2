using System.Data.Entity;

namespace PAW_P2.Models
{
    // Contexto de Entity Framework para el Sistema Académico
    public class SistemaAcademicoContexto : DbContext
    {
        // Constructor que usa la cadena de conexión del Web.config
        public SistemaAcademicoContexto() : base("SistemaAcademicoConexion")
        {
            // Desactivar la inicialización automática
            Database.SetInitializer<SistemaAcademicoContexto>(null);
        }

        // DbSets para cada entidad
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<CursoDocente> CursoDocentes { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Calificacion> Calificaciones { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }

        // Configuración del modelo
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar índice único para el código de curso
            modelBuilder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            // Configurar índice único para el correo de usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            // Configurar índice único para identificación de usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Identificacion)
                .IsUnique();

            // Configurar relación Usuario-Rol
            modelBuilder.Entity<Usuario>()
                .HasRequired(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .WillCascadeOnDelete(false);

            // Configurar relación CursoDocente
            modelBuilder.Entity<CursoDocente>()
                .HasRequired(cd => cd.Curso)
                .WithMany(c => c.DocentesAsignados)
                .HasForeignKey(cd => cd.CursoId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CursoDocente>()
                .HasRequired(cd => cd.Docente)
                .WithMany(u => u.CursosAsignados)
                .HasForeignKey(cd => cd.DocenteId)
                .WillCascadeOnDelete(false);

            // Configurar relación Inscripcion
            modelBuilder.Entity<Inscripcion>()
                .HasRequired(i => i.Estudiante)
                .WithMany(u => u.Inscripciones)
                .HasForeignKey(i => i.EstudianteId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Inscripcion>()
                .HasRequired(i => i.Curso)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(i => i.CursoId)
                .WillCascadeOnDelete(false);

            // Configurar precisión decimal para notas
            modelBuilder.Entity<Calificacion>()
                .Property(c => c.Nota)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Calificacion>()
                .Property(c => c.Porcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Inscripcion>()
                .Property(i => i.NotaFinal)
                .HasPrecision(5, 2);
        }
    }
}
