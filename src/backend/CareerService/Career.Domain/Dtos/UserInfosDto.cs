﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public sealed record UserInfosDto
    {
        public string id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public DateTime createdAt { get; set; }
    }

    public sealed record UserInfosWithRolesDto
    {
        public string id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public List<UserRolesDto> userRoles { get; set; }
        public DateTime createdAt { get; set; }
    }
}
