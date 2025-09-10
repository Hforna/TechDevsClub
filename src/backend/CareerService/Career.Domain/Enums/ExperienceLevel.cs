using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Enums
{
    public enum ExperienceLevel
    {
        [Display(Name = "No experience")]
        NoExperience,

        [Display(Name = "Less than 1 year")]
        LessThanOneYear,

        [Display(Name = "1-2 years")]
        OneToTwoYears,

        [Display(Name = "2-5 years")]
        TwoToFiveYears,

        [Display(Name = "5+ years")]
        FivePlusYears,

        [Display(Name = "10+ years")]
        TenPlusYears
    }
}
