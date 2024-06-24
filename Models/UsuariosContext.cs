using Microsoft.EntityFrameworkCore;

namespace InnovaTubeBack.Models
{
    public class UsuariosContext:DbContext
    {
        public UsuariosContext(DbContextOptions<UsuariosContext> options)
        : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
    }
}
