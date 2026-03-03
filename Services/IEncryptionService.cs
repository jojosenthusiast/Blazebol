namespace ControlTorneosFutbol.Services
{
    /// <summary>
    /// Servicio para cifrado/descifrado de datos sensibles usando AES-256
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Cifra un texto plano usando AES-256
        /// </summary>
        /// <param name="plainText">Texto a cifrar</param>
        /// <returns>Texto cifrado en Base64</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Descifra un texto cifrado
        /// </summary>
        /// <param name="cipherText">Texto cifrado en Base64</param>
        /// <returns>Texto plano descifrado</returns>
        string Decrypt(string cipherText);

        /// <summary>
        /// Hashea una contraseña usando BCrypt
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash de la contraseña</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifica si una contraseña coincide con su hash
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="hashedPassword">Hash almacenado</param>
        /// <returns>True si coincide, false si no</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}