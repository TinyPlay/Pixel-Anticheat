/*
 * Pixel Anti Cheat
 * ======================================================
 * This library allows you to organize a simple anti-cheat
 * for your game and take care of data security. You can
 * use it in your projects for free.
 *
 * Note that it does not guarantee 100% protection for
 * your game. If you are developing a multiplayer game -
 * never trust the client and check everything on
 * the server.
 * ======================================================
 * @developer       TinyPlay
 * @author          Ilya Rastorguev
 * @url             https://github.com/TinyPlay/Pixel-Anticheat
 */
namespace PixelAnticheat.Data
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEngine.Events;
    using PixelAnticheat.Detectors;
    using PixelAnticheat.Encryption;
    using PixelAnticheat.SecuredTypes;
    
    /// <summary>
    /// Secured File Reading / Writing Class
    /// </summary>
    public static class SecuredFile
    {
        // Private Params
        private static string _encryptionKey = "A&fv2hAD9jgkdf89^ASD2q89zsjdA"; // Default Data Encryption Key
        
        /// <summary>
        /// Update Encryption Key
        /// </summary>
        /// <param name="key"></param>
        public static void UpdateEncryptionKey(string key)
        {
            _encryptionKey = key;
        }

        /// <summary>
        /// Get Encryption Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetEnryptionKey()
        {
            return _encryptionKey;
        }

        #region Read Files
        /// <summary>
        /// Reaa file from path
        /// </summary>
        /// <param name="path">Path to File</param>
        /// <param name="encoding">File Encoding</param>
        /// <returns>String Data</returns>
        public static string ReadFile(string path, Encoding encoding = null)
        {
            if(File.Exists(path))
            {
                if (encoding != null)
                {
                    return File.ReadAllText(path, encoding);
                }
                else
                {
                    return File.ReadAllText(path);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Read Encrypted File
        /// </summary>
        /// <param name="path">Path to File</param>
        /// <param name="encryptionType">Encryption Type</param>
        /// <param name="encoding">File Encoding</param>
        /// <returns>Decrypted string or hash or null</returns>
        public static string ReadEncryptedFile(string path,
            FileEncryptionType encryptionType = FileEncryptionType.Base64, Encoding encoding = null)
        {
            string decryptedData = null;
            if (File.Exists(path))
            {
                decryptedData = (encoding != null) ? File.ReadAllText(path, encoding) : File.ReadAllText(path);
                switch (encryptionType)
                {
                    case FileEncryptionType.Base64:
                        decryptedData = Base64.Decode(decryptedData);
                        break;
                    case FileEncryptionType.MD5:
                        Debug.LogWarning("Atention! You cannot decrypt hashed files. This hash will be returned as is.");
                        break;
                    case FileEncryptionType.AES:
                        decryptedData = AES.Decrypt(decryptedData, _encryptionKey);
                        break;
                    case FileEncryptionType.RSA:
                        decryptedData = RSA.Decrypt(decryptedData, _encryptionKey);
                        break;
                    case FileEncryptionType.SHA:
                        Debug.LogWarning("Atention! You cannot decrypt hashed files. This hash will be returned as is.");
                        break;
                    default:
                        Debug.LogError("Failed to read encrypted file. Unknown encryption type.");
                        break;
                }
            }

            return decryptedData;
        }

        /// <summary>
        /// Read Binary Data
        /// </summary>
        /// <param name="path">Path to Binary File</param>
        /// <returns>byte array or null</returns>
        public static byte[] ReadBinary(string path)
        {
            if(File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Read Encrypted Binary (Supported only for Base64, RSA and AES methods)
        /// </summary>
        /// <param name="path">Path to Binary File</param>
        /// <param name="encryptionType">Encryption Type</param>
        /// <returns></returns>
        public static byte[] ReadEncryptedBinary(string path,
            FileEncryptionType encryptionType = FileEncryptionType.AES)
        {
            byte[] decryptedData = null;
            if(File.Exists(path))
            {
                decryptedData = File.ReadAllBytes(path);
                switch (encryptionType)
                {
                    case FileEncryptionType.Base64:
                        decryptedData = Base64.DecodeBinary(File.ReadAllText(path));
                        break;
                    case FileEncryptionType.MD5:
                        Debug.LogError("Failed to Read Encrypted Binary from MD5. Unsupported encryption method.");
                        break;
                    case FileEncryptionType.SHA:
                        Debug.LogError("Failed to Read Encrypted Binary from SHA. Unsupported encryption method.");
                        break;
                    case FileEncryptionType.AES:
                        decryptedData = AES.Decrypt(decryptedData, _encryptionKey);
                        break;
                    case FileEncryptionType.RSA:
                        decryptedData = RSA.Decrypt(decryptedData, _encryptionKey);
                        break;
                    default:
                        Debug.LogError("Failed to read encrypted Binary. Unknown encryption type.");
                        break;
                }
            }
            
            return decryptedData;
        }

        /// <summary>
        /// Read File from Resources
        /// </summary>
        /// <param name="resource">Resource Name / Path</param>
        /// <returns></returns>
        public static string ReadFileFromResources(string resource)
        {
            TextAsset output = Resources.Load<TextAsset>(resource);
            return output.text;
        }

        /// <summary>
        /// Read Encrypted File from Resources
        /// </summary>
        /// <param name="resource">Resource Name / Path</param>
        /// <param name="encryptionType">Encryption Type</param>
        /// <returns></returns>
        public static string ReadEncryptedFileFromResources(string resource,
            FileEncryptionType encryptionType = FileEncryptionType.Base64)
        {
            TextAsset output = Resources.Load<TextAsset>(resource);
            string decryptedData = output.text;
            switch (encryptionType)
            {
                case FileEncryptionType.Base64:
                    decryptedData = Base64.Decode(decryptedData);
                    break;
                case FileEncryptionType.MD5:
                    Debug.LogWarning(
                        "Atention! You cannot decrypt hashed files. This hash will be returned as is.");
                    break;
                case FileEncryptionType.AES:
                    decryptedData = AES.Decrypt(decryptedData, _encryptionKey);
                    break;
                case FileEncryptionType.RSA:
                    decryptedData = RSA.Decrypt(decryptedData, _encryptionKey);
                    break;
                case FileEncryptionType.SHA:
                    Debug.LogWarning(
                        "Atention! You cannot decrypt hashed files. This hash will be returned as is.");
                    break;
                default:
                    Debug.LogError("Failed to read encrypted file. Unknown encryption type.");
                    break;
            }

            return decryptedData;
        }

        /// <summary>
        /// Read Binary from Resources
        /// </summary>
        /// <param name="resource">Resource Name / Path</param>
        /// <returns></returns>
        public static byte[] ReadBinaryFromResources(string resource)
        {
            TextAsset output = Resources.Load<TextAsset>(resource);
            return output.bytes;
        }

        /// <summary>
        /// Read Binary from Resources (Supported only for Base64, RSA and AES methods)
        /// </summary>
        /// <param name="resource">Resource Name / Path</param>
        /// <param name="encryptionType">Encryption Type</param>
        /// <returns></returns>
        public static byte[] ReadEncryptedBinaryFromResources(string resource,
            FileEncryptionType encryptionType = FileEncryptionType.AES)
        {
            TextAsset output = Resources.Load<TextAsset>(resource);
            byte[] decryptedData = output.bytes;
            switch (encryptionType)
            {
                case FileEncryptionType.Base64:
                    decryptedData = Base64.DecodeBinary(output.text);
                    break;
                case FileEncryptionType.MD5:
                    Debug.LogError("Failed to Read Encrypted Binary from MD5. You can't decode hash.");
                    break;
                case FileEncryptionType.SHA:
                    Debug.LogError("Failed to Read Encrypted Binary from SHA. You can't decode hash.");
                    break;
                case FileEncryptionType.AES:
                    decryptedData = AES.Decrypt(decryptedData, _encryptionKey);
                    break;
                case FileEncryptionType.RSA:
                    decryptedData = RSA.Decrypt(decryptedData, _encryptionKey);
                    break;
                default:
                    Debug.LogError("Failed to read encrypted Binary. Unknown encryption type.");
                    break;
            }
            return decryptedData;
        }
        #endregion

        #region Save Files
        /// <summary>
        /// Save File
        /// </summary>
        /// <param name="path">Path to File</param>
        /// <param name="content">Text Content to Save</param>
        /// <param name="encoding">File Encoding</param>
        public static void SaveFile(string path, string content, Encoding encoding = null)
        {
            if (encoding != null)
            {
                File.WriteAllText(path, content, encoding);
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }

        /// <summary>
        /// Save Encrypted File
        /// </summary>
        /// <param name="path">Path to File</param>
        /// <param name="content">Text Content to Save</param>
        /// <param name="encoding">File Encoding</param>
        /// <param name="encryptionType">Encryption Type</param>
        public static void SaveEncryptedFile(string path, string content, FileEncryptionType encryptionType = FileEncryptionType.Base64,
            Encoding encoding = null)
        {
            switch (encryptionType)
            {
                case FileEncryptionType.Base64:
                    content = Base64.Encode(content);
                    return;
                case FileEncryptionType.MD5:
                    content = MD5.GetHash(content);
                    break;
                case FileEncryptionType.SHA:
                    content = SHA.GetSHA256Hash(content);
                    break;
                case FileEncryptionType.AES:
                    content = AES.Encrypt(content, _encryptionKey);
                    break;
                case FileEncryptionType.RSA:
                    content = RSA.Encrypt(content, _encryptionKey);
                    break;
                default:
                    Debug.LogError("Failed to save Binary with Encryption. Unknown encryption type.");
                    break;
            }
            
            if (encoding != null)
            {
                File.WriteAllText(path, content, encoding);
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }

        /// <summary>
        /// Save Binary data to File
        /// </summary>
        /// <param name="path">Path to File</param>
        /// <param name="content">Binary Data to Save</param>
        public static void SaveBinary(string path, byte[] content)
        {
            File.WriteAllBytes(path, content);
        }

        /// <summary>
        /// Save Encrypted Binary data to File
        /// </summary>
        /// <param name="path">Path to File</param>
        /// <param name="content">Binary Data to Save</param>
        /// <param name="encryptionType">Encryption Type</param>
        public static void SaveEncryptedBinary(string path, byte[] content,
            FileEncryptionType encryptionType = FileEncryptionType.AES)
        {
            switch (encryptionType)
            {
                case FileEncryptionType.Base64:
                    string binaryEncoded = Base64.EncodeBinary(content);
                    File.WriteAllText(path, binaryEncoded);
                    return;
                case FileEncryptionType.MD5:
                    content = MD5.GetBinaryHash(content);
                    break;
                case FileEncryptionType.SHA:
                    content = SHA.GetSHA256ByteHash(content);
                    break;
                case FileEncryptionType.AES:
                    content = AES.Encrypt(content, _encryptionKey);
                    break;
                case FileEncryptionType.RSA:
                    content = RSA.Encrypt(content, _encryptionKey);
                    break;
                default:
                    Debug.LogError("Failed to save Binary with Encryption. Unknown encryption type.");
                    break;
            }
            File.WriteAllBytes(path, content);
        }
        #endregion

        #region Remove Files
        /// <summary>
        /// Remove File from Path
        /// </summary>
        /// <param name="path">Path to File</param>
        public static void RemoveFile(string path)
        {
            if(File.Exists(path)) File.Delete(path);
        }

        /// <summary>
        /// Copy File
        /// </summary>
        /// <param name="originalFilePath"></param>
        /// <param name="destinationFilePath"></param>
        /// <param name="overwrite"></param>
        public static void CopyFile(string originalFilePath, string destinationFilePath, bool overwrite = false)
        {
            if (File.Exists(originalFilePath))
            {
                File.Copy(originalFilePath, destinationFilePath, overwrite);
            }
        }
        #endregion
        
        /// <summary>
        /// File Encryption Type
        /// </summary>
        public enum FileEncryptionType
        {
            AES,
            Base64,
            MD5,
            RSA,
            SHA
        }
    }
}