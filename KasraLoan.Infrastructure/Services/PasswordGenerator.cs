using KasraLoan.Application.Services.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Services
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Digits = "23456789";
        private const string Symbols = "!@#$%^&*";

        public string Generate(int length = 12)
        {
            if (length < 8)
                length = 8;

            var allChars = Lowercase + Uppercase + Digits + Symbols;

            var result = new StringBuilder(length);

            // اطمینان از این‌که حداقل یک کاراکتر از هر دسته وجود دارد
            result.Append(GetRandomChar(Lowercase));
            result.Append(GetRandomChar(Uppercase));
            result.Append(GetRandomChar(Digits));
            result.Append(GetRandomChar(Symbols));

            for (int i = result.Length; i < length; i++)
            {
                result.Append(GetRandomChar(allChars));
            }

            return Shuffle(result.ToString());
        }

        private static char GetRandomChar(string source)
        {
            var index = RandomNumberGenerator.GetInt32(source.Length);
            return source[index];
        }

        private static string Shuffle(string input)
        {
            var chars = input.ToCharArray();

            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}