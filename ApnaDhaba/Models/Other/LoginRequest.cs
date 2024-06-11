using System.ComponentModel.DataAnnotations;

namespace ApnaDhaba.Models.Other
{
    public class LoginRequest
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}