﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Profile.Domain.Entities;

namespace Profile.Domain.Aggregates
{
    [Table("devices")]
    public class Device : IEntity
    {
        public long Id { get; set; }
    }
}
