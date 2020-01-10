using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ThAmCo.Events.Models
{
    public class EventEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}
