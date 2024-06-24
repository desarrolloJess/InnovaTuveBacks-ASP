using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InnovaTubeBack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace InnovaTubeBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosContext _context;
        private readonly IConfiguration _configuration; // Inyectar IConfiguration

        public UsuariosController(UsuariosContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // Inicializar IConfiguration
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Usuarios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        // POST: api/Usuarios/registrarUsuario
        [HttpPost("registrarUsuario")]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario, string recaptchaToken)
        {

            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == usuario.CorreoElectronico);
            if (existingUser != null)
            {
                return BadRequest("El correo electrónico ya está asociado a una cuenta.");
            }

            // Validar el token de reCAPTCHA
            var isRecaptchaValid = await ValidateRecaptcha(recaptchaToken);
            if (!isRecaptchaValid)
            {
                return BadRequest("Token de reCAPTCHA no válido.");
            }

            // Guardar el usuario en la base de datos
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Devolver respuesta con el usuario creado
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        private async Task<bool> ValidateRecaptcha(string recaptchaToken)
        {
            try
            {
                var secretKey = "6Lddw_8pAAAAAMtMirIowW6oZUBLC2y8DhQqBnHM"; 

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaToken}", null);
                response.EnsureSuccessStatusCode();

                var stringResponse = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(stringResponse);

                return jsonResponse.Value<bool>("success");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al validar reCAPTCHA: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al validar reCAPTCHA: {ex.Message}");
                return false;
            }
        }

        [HttpPost("validarUsuario")]
        public async Task<ActionResult<Usuario>> ValidarUsuario(UsuarioLogin usuarioLogin)
        {
            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == usuarioLogin.CorreoElectronico);

            if (existingUser != null && existingUser.Contrasenia == usuarioLogin.Contrasenia)
            {
                return Ok(existingUser); 
            }

            return NotFound("Usuario no encontrado o contraseña incorrecta");
        }

        public class UsuarioLogin
        {
            public string CorreoElectronico { get; set; }
            public string Contrasenia { get; set; }
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
