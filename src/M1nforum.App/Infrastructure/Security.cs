using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System;

namespace M1nforum.Web.Infrastructure
{
	// adapted from https://github.com/chandru415/password-hash-verify-CSharp
	public static class Security
	{
		public static string GenerateHashPassword(string password, byte[] salt = null)
		{
			if (salt == null || salt.Length != 16)
			{
				// generate a 128-bit salt using a secure PRNG
				salt = new byte[128 / 8];
				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(salt);
				}
			}

			string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: password,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: 10000,
				numBytesRequested: 256 / 8));

			// password will be concatenated with salt using ':'
			return $"{hashed}:{Convert.ToBase64String(salt)}";
		}

		public static bool VerifyPassword(string hashedPasswordWithSalt, string passwordToCheck)
		{
			var passwordAndHash = hashedPasswordWithSalt.Split(':');
			if (passwordAndHash == null || passwordAndHash.Length != 2)
			{
				return false;
			}

			var salt = Convert.FromBase64String(passwordAndHash[1]);
			if (salt == null)
			{
				return false;
			}

			var hashOfpasswordToCheck = GenerateHashPassword(passwordToCheck, salt);

			return string.Compare(hashedPasswordWithSalt, hashOfpasswordToCheck) == 0;
		}
	}
}

