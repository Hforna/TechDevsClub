﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public class UserRolesDto
    {
        public string userId { get; set; }
        public List<UserRoleDto> roles { get; set; }
    }
}
