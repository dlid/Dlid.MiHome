using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Dlid.MiHome.Protocol.Helpers
{
    internal class AESHelper
    {

        public static byte[] Encrypt(byte[] key, byte[] iv, string message)
        {
            var _aes = new AesCryptoServiceProvider();
            _aes.BlockSize = 128;
            _aes.KeySize = 128;
            _aes.Key = key;
            _aes.IV = iv;
            _aes.Padding = PaddingMode.PKCS7;
            _aes.Mode = CipherMode.CBC;

            var _crypto = _aes.CreateEncryptor(_aes.Key, _aes.IV);
            byte[] encrypted = _crypto.TransformFinalBlock(
                ASCIIEncoding.UTF8.GetBytes(message), 0, ASCIIEncoding.UTF8.GetBytes(message).Length);
            _crypto.Dispose();
            return encrypted;
        }

        public static byte[] Decrypt(byte[] key, byte[] iv, byte[] message)
        {
            var _aes = new AesCryptoServiceProvider();
            _aes.BlockSize = 128;
            _aes.KeySize = 128;
            _aes.Key = key;
            _aes.IV = iv;
            _aes.Padding = PaddingMode.PKCS7;
            _aes.Mode = CipherMode.CBC;

            var _crypto = _aes.CreateDecryptor(_aes.Key, _aes.IV);
            byte[] decrypted = _crypto.TransformFinalBlock(
               message, 0, message.Length);
            _crypto.Dispose();
            return decrypted;
        }
    }
}
