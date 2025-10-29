using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaTune.Helpers
{
    public static class VerificationHelper
    {
        public static string HideEmail(string email)
        {
            var parts = email.Split('@');
            var builder = new StringBuilder();
            for (int i = 0; i < parts[0].Length; i++)
            {
                if (i < 3 || i > parts[0].Length - 4) builder.Append(parts[0][i]);
                else builder.Append('*');
            }
            return $"{builder}@{parts[1]}";
        }
    }
}
