using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Events.Services;

namespace ThAmCo.Events.Models
{
    public class VenuesGetViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan? Duration { get; set; }

        public string TypeId { get; set; }

        public IEnumerable<AvailabilityGetDto> Venues { get; set; }
    }
}
