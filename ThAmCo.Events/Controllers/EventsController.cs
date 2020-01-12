using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;
using ThAmCo.Events.Models;
using ThAmCo.Events.Services;

namespace ThAmCo.Events.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventsDbContext _context;

        public EventsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }


        // GET: Events/Details/5
        public async Task<IActionResult> EventDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            IEnumerable<GuestBooking> Bookings = new List<GuestBooking>();

            var eventsDBContext = _context.Guests
                .Include(g => g.Customer)
                .AsQueryable()
                .Include(g => g.Event)
                .AsQueryable();

            eventsDBContext = eventsDBContext
                .Where(e => e.EventId == id.Value);

            Bookings = await eventsDBContext.ToListAsync();

            if (@event.Reference == null)
            {
                @event.Reference = "Null";

                var eventDetailsWithoutVenue  = new EventVenueGuestViewModel()
                {
                    EventId = @event.Id,
                    Title = @event.Title,
                    Date = @event.Date,
                    Duration = @event.Duration,
                    TypeId = @event.TypeId,
                    Bookings = Bookings,
                    Reference = @event.Reference
                };
                return View(eventDetailsWithoutVenue);
            }

            var EventDetails = new EventVenueGuestViewModel()
            {
                EventId = @event.Id,
                Title = @event.Title,
                Date = @event.Date,
                Duration = @event.Duration,
                TypeId = @event.TypeId,
                Bookings = Bookings,
                Reference = @event.Reference
            };

            return View(EventDetails);
        }



        // GET: Events/GetVenues/5
        public async Task<IActionResult> GetVenues(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null)
            {
                return NotFound();
            }


            IEnumerable<AvailabilityGetDto> reservation = new List<AvailabilityGetDto>();
            HttpClient client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:23652");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            try
            {
                // api/Availability?eventType=X?beginDate=X&endDate=X
                var uri = @"/api/Availability/?eventType=" + @event.TypeId + "&beginDate=" + @event.Date.ToString("yyyy-MM-dd") + "&endDate=" + @event.Date.AddDays(1).ToString("yyyy-MM-dd");

                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    reservation = await response.Content.ReadAsAsync<IEnumerable<AvailabilityGetDto>>();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                Debug.WriteLine("Details received a bad response from the web service.");
            }

            var eventVenues = new VenuesGetViewModel() {
                Id = @event.Id,
                Date = @event.Date,
                Duration = @event.Duration,
                Title = @event.Title,
                TypeId = @event.TypeId,
                Venues = reservation
            };
            
            return View(eventVenues);

        }


        // POST: Events/GetVenues/5
        [HttpPost, ActionName("GetVenues")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetVenues(int? id, string toReserve)
        {

            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            if (toReserve == null)
            {
                return RedirectToAction("GetVenues", new { id });
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:23652");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            var uri = "/api/reservations/";

            if (@event.Reference == null)
            {

                var reservedPost = new ReservationPostDto
                {
                    EventDate = @event.Date,
                    VenueCode = toReserve,
                    StaffId = "NULL"
                };
                if (reservedPost.VenueCode == null||reservedPost.StaffId == null)
                {
                    return RedirectToAction("GetVenues", new { id });
                }

                HttpResponseMessage response = await client.PostAsJsonAsync(uri, reservedPost);
                if (response.IsSuccessStatusCode)
                {
                    @event.Reference = $"{toReserve}{@event.Date:yyyyMMdd}";

                    _context.Update(@event);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw new Exception();
                } 
            }
            else
            {
                if (@event.Reference != $"{toReserve}{@event.Date:yyyyMMdd}")
                {
                    var referenceDelete = @event.Reference;

                    HttpResponseMessage responseDelete = await client.DeleteAsync(uri + referenceDelete);
                    if (responseDelete.IsSuccessStatusCode)
                    {
                        var reservedPost = new ReservationPostDto
                        {
                            EventDate = @event.Date,
                            VenueCode = toReserve,
                            StaffId = "NULL"
                        };

                        HttpResponseMessage response = await client.PostAsJsonAsync(uri, reservedPost);
                        if (response.IsSuccessStatusCode)
                        {
                            @event.Reference = $"{toReserve}{@event.Date:yyyyMMdd}";

                            _context.Update(@event);
                            await _context.SaveChangesAsync();
                            RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
            return RedirectToAction("GetVenues", new {id});
        }
        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Date,Duration,TypeId")] Event @event)
        {
            
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Duration")] EventEditViewModel @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var editEvent = await _context.Events.FindAsync(id);
                    editEvent.Title = @event.Title;
                    editEvent.Duration = @event.Duration;
                    _context.Update(editEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
