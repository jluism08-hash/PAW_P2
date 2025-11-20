using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using Microsoft.IdentityModel.Tokens;
using PAW_P2.Models;
using PAW_P2.Servicios;

namespace PAW_P2.Controllers.Api
{
    // Controlador API para autenticación
    [RoutePrefix("api/autenticacion")]
    public class AutenticacionApiController : ApiController
    {
        private readonly ServicioAutenticacion _servicioAuth;
        private readonly ServicioBitacora _servicioBitacora;

        public AutenticacionApiController()
        {
            _servicioAuth = new ServicioAutenticacion();
            _servicioBitacora = new ServicioBitacora();
        }

        // POST api/autenticacion/login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginViewModel modelo)
        {
            if (modelo == null || string.IsNullOrEmpty(modelo.Correo) || string.IsNullOrEmpty(modelo.Contrasena))
            {
                return BadRequest("Correo y contraseña son requeridos");
            }

            var usuario = _servicioAuth.Autenticar(modelo.Correo, modelo.Contrasena);

            if (usuario == null)
            {
                return Unauthorized();
            }

            // Generar token JWT
            var token = GenerarTokenJwt(usuario);

            // Registrar en bitácora
            _servicioBitacora.RegistrarInicioSesion(usuario.UsuarioId, usuario.NombreCompleto);

            // Obtener permisos del usuario
            var permisos = _servicioAuth.ObtenerPermisos(usuario.UsuarioId);

            return Ok(new
            {
                token = token,
                usuario = new
                {
                    id = usuario.UsuarioId,
                    nombre = usuario.NombreCompleto,
                    correo = usuario.Correo,
                    rol = usuario.Rol.Nombre,
                    permisos = permisos
                }
            });
        }

        // Método para generar token JWT
        private string GenerarTokenJwt(Usuario usuario)
        {
            var claveSecreta = ConfigurationManager.AppSettings["JwtSecretKey"];
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["JwtAudience"];
            var expiracionMinutos = int.Parse(ConfigurationManager.AppSettings["JwtExpirationMinutes"]);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
                new Claim("RolId", usuario.RolId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiracionMinutos),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _servicioAuth?.Dispose();
                _servicioBitacora?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ViewModel para el login
    public class LoginViewModel
    {
        public string Correo { get; set; }
        public string Contrasena { get; set; }
    }
}
