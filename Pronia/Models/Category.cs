﻿using System.ComponentModel.DataAnnotations;

namespace Pronia.Models
{
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "The field is required!")]
        [MaxLength(30, ErrorMessage = "There can be max 30 symbols!")]
        public string Name { get; set; }

        //relational
        public List<Product>? Products { get; set; }
    }
}
