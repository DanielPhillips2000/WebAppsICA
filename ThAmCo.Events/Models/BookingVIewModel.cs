using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ThAmCo.Events.Models
{
    public class BookingViewModel
    {
        [Display(Name ="Event Id")]
        public int EventId { get; set; }

        public string Event { get; set; }

        public bool Attended { get; set; }
    }
}
