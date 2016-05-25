using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SmallId.Code
{
    public class RegistryMembershipService : IMembershipService
    {
        private readonly RegistryKey regKey;
        private readonly ConcurrentDictionary<string, Guid> usernameToGuidMap = new ConcurrentDictionary<string, Guid>();

        public RegistryMembershipService()
        {
            var hklmKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            regKey = hklmKey.OpenSubKey(@"SOFTWARE\SmallId", RegistryKeyPermissionCheck.ReadWriteSubTree);
            
            if (regKey == null)
            {
                throw new InvalidOperationException(@"Could not open the registry key: HKLM\SOFTWARE\SmallId");
            }

            Guid userGuid;
            foreach (var userGuidValue in regKey.GetValueNames())
            {
                if (!Guid.TryParseExact(userGuidValue, "B", out userGuid))
                {
                    continue;
                }
                var username = regKey.GetValue(userGuidValue) as string;
                if (username != null)
                {
                    usernameToGuidMap.TryAdd(username, userGuid);
                }
            }
        }

        public bool ValidateUser(string username, string password)
        {
            Guid userGuid;
            if (!usernameToGuidMap.TryGetValue(username, out userGuid))
            {
                return false;
            }
            // lookup user's hash from the registry
            var userKey = regKey.OpenSubKey(userGuid.ToString("B"));
            if (userKey == null)
            {
                return false;
            }
            byte[] hashedPassword = userKey.GetValue("HashedPassword") as byte[];
            byte[] salt = userKey.GetValue("Salt") as byte[];
            if (hashedPassword == null || salt == null)
            {
                return false;
            }
            
            return hashedPassword.SequenceEqual(GeneratePasswordHash(password, salt));
        }

        public bool RegisterNewUser(string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            if (usernameToGuidMap.ContainsKey(username.ToLowerInvariant()))
            {
                return false;
            }

            username = username.ToLowerInvariant();

            byte[] salt;
            byte[] hashedPassword;
            GenerateNewPasswordHash(password, out salt, out hashedPassword);

            // add a value from this new user
            Guid userGuid = Guid.NewGuid();
            regKey.SetValue(userGuid.ToString("B"), username, RegistryValueKind.String);
            var userKey = regKey.CreateSubKey(userGuid.ToString("B"));
            userKey.SetValue("Username", username, RegistryValueKind.String);
            userKey.SetValue("Email", email, RegistryValueKind.String);
            userKey.SetValue("HashedPassword", hashedPassword, RegistryValueKind.Binary);
            userKey.SetValue("Salt", salt, RegistryValueKind.Binary);

            usernameToGuidMap.TryAdd(username, userGuid);

            return true;
        }

        private void GenerateNewPasswordHash(string password, out byte[] salt, out byte[] hashedPassword)
        {
            // create a random salt
            salt = new byte[8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }

            hashedPassword = GeneratePasswordHash(password, salt);
        }

        private byte[] GeneratePasswordHash(string password, byte[] salt)
        {
            using (HashAlgorithm algorithm = new SHA512Managed())
            {
                var plainText = Encoding.Unicode.GetBytes(password);

                var plainTextWithSaltBytes = new byte[plainText.Length + salt.Length];

                for (var i = 0; i < plainText.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainText[i];
                }
                for (var i = 0; i < salt.Length; i++)
                {
                    plainTextWithSaltBytes[plainText.Length + i] = salt[i];
                }

                return algorithm.ComputeHash(plainTextWithSaltBytes);
            }
        }
    }
}