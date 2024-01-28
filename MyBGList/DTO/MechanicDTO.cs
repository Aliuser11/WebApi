﻿using System.ComponentModel.DataAnnotations;
using MyBGList.Attributes;

namespace MyBGList.DTO
{
    public class MechanicDTO
    {
        [Required]
        public int Id { get; set; }

        [LettersOnlyValidatorAttribute(UseRegex = true)]
        public string? Name { get; set; } 

    }
}
