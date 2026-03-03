using Microsoft.Extensions.Configuration;
using System;

namespace ControlTorneosFutbol.Helpers
{
    public static class ConfigurationValidator
    {
        public static void ValidateConfiguration(IConfiguration configuration)
        {
            var errors = new List<string>();

            // Validar ConnectionString
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                errors.Add("ConnectionString 'DefaultConnection' no está configurada");
            }
            else if (!connectionString.Contains("supabase.co") && !connectionString.Contains("localhost") && !connectionString.Contains("127.0.0.1"))
            {
                errors.Add("ConnectionString parece inválida");
            }

            // Validar clave de cifrado
            var encryptionKey = configuration["Encryption:Key"];
            if (string.IsNullOrEmpty(encryptionKey))
            {
                errors.Add("Encryption:Key no está configurada");
            }
            else if (encryptionKey.Length != 32)
            {
                errors.Add($"Encryption:Key debe tener exactamente 32 caracteres (actual: {encryptionKey.Length})");
            }

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    "Errores de configuración detectados:\n" + string.Join("\n", errors)
                );
            }
        }
    }
}