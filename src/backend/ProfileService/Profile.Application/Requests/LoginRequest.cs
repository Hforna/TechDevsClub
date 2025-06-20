﻿using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public sealed record LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
