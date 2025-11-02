using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace ClientsService.Models
{
    public class Client
    {
        public Guid Id { get; set; }
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty; // Unique
        [Required]
        public string Username { get; set; } = string.Empty; // Unique
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public DateOnly BirthDate { get; set; } = DateOnly.MinValue;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = "active";
        [Required]
        public string Role { get; set; } = "client";
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
}