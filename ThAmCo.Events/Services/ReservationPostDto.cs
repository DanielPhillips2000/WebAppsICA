﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ThAmCo.Events.Services
{
    public class ReservationPostDto
    {
        [Required]
        public DateTime EventDate { get; set; }

        [Required, MinLength(5), MaxLength(5)]
        public string VenueCode { get; set; }

        [Required]
        public string StaffId { get; set; }
    }
}
