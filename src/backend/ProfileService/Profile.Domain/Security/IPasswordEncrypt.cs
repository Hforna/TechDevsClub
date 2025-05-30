using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Security
{
    public interface IPasswordEncrypt
    {
        public string Encrypt(string password);

        public bool IsValidPassword(string password, string hash);
    }
}
