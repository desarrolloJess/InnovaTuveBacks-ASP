using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InnovaTubeBack.Models
{
    public class Usuario
    {

        [Key]
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string NombreUsuario { get; set; }
        public string CorreoElectronico { get; set; }
        public string Contrasenia { get; set;}
    }
}
