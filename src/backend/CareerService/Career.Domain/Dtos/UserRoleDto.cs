﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public sealed record UserRoleDto
    {
        public string userId { get; set; }
        public string roleName { get; set; }
    }
}
