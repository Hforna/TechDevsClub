using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.ValueObjects
{
    public sealed record Salary(decimal MinSalary, decimal MaxSalary, Currency Currency)
    {
    }
}
