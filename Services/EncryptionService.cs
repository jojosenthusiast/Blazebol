using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace ControlTorneosFutbol.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly string _encryptionKey;
        private readonly byte[] _keyBytes;
        private readonly byte[] _ivBytes;

        public EncryptionService(IConfiguration configuration)
        {
            // Obtener clave de configuración
            _encryptionKey = configuration["Encryption:Key"] 
                ?? throw new InvalidOperationException("Encryption:Key no está configurada");

            // Validar longitud de clave (debe ser 32 bytes para AES-256)
            if (_encryptionKey.Length != 32)
            {
                throw new InvalidOperationException("La clave de cifrado debe tener exactamente 32 caracteres");
            }

            _keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
            
            // IV fijo (en producción, considerar IV aleatorio por registro)
            // Por simplicidad, usamos los primeros 16 bytes de la clave
            _ivBytes = new byte[16];
            Array.Copy(_keyBytes, _ivBytes, 16);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _keyBytes;
                    aes.IV = _ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al cifrar datos", ex);
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = _keyBytes;
                    aes.IV = _ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(buffer))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al descifrar datos", ex);
            }
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // BCrypt genera automáticamente un salt y lo incluye en el hash
            // WorkFactor 12 es un buen balance entre seguridad y rendimiento
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}