using System.ComponentModel.DataAnnotations;

namespace ApnaDhaba.Models.Other
{
    public class Reset
    {
        [Required]
        public string? username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}