﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace MyBGList.DTO
{
    public class DomainDTO
    {
        [Required]
        public int Id { get; set; } 

        public string? Name { get; set; }

    }
}

