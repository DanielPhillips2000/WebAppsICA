using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ThAmCo.Events.Services
{
    public class AvailabilityGetDto
    {
        [Key, MinLength(5), MaxLength(5)]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Range(1, Int32.MaxValue)]
        public int Capacity { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Range(0.0, Double.MaxValue)]
        public double CostPerHour { get; set; }
    }
}
