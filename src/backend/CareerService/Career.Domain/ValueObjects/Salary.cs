using Career.Domain.Enums;
using Career.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.ValueObjects
{
    public sealed record Salary
    {
        public Salary(decimal minSalary, decimal maxSalary, ECurrency currency)
        {
            if (minSalary > maxSalary)
                throw new DomainException(ResourceExceptMessages.MIN_SALARY_LOWER_THAN_HIGHER);

            MinSalary = minSalary;
            MaxSalary = maxSalary;
            Currency = currency;
        }

        public decimal MinSalary { get; private set; }
        public decimal MaxSalary { get; private set; }
        public ECurrency Currency { get; private set; }
    }
}
