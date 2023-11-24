﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RestAlpaka.Model
{
    public class Customers
    {
        [Key]
        public int Customer_id { get; set; }

        [ForeignKey("User")]
        public int User_id { get; set; }

        [ForeignKey("Location")]
        public int Location_id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 50 characters.")]
        public string First_name { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 50 characters.")]
        public string Last_name { get; set; }

        [StringLength(20, ErrorMessage = "Phone number must be up to 20 characters.")]
        public string Phone_number { get; set; }

        [StringLength(100, ErrorMessage = "Address must be up to 100 characters.")]
        public string Address { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public Location Location { get; set; }
    }
}

