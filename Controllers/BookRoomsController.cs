using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Xml.Linq;

namespace Booking.Controllers
{
    public class BookRoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RoomsController> _logger;
        public BookRoomsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<RoomsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: BookRooms
        public IActionResult Index()
        {
            try
            {
                if (_context.Rooms == null)
                {
                    return View("Error");
                }

                var rooms = _context.Rooms
                    .Select(room => new SelectListItem
                    {
                        Value = room.Id.ToString(),
                        Text = room.Name 
                    })
                    .ToList();
                ViewBag.UnavailableDates = GetUnavailableDates(); 
                ViewBag.Rooms = rooms;

                var bookRooms = new BookRooms
                {
                };

                return View(bookRooms);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }



        private List<string> GetUnavailableDates()
        {
            var bookedDates = _context.BookRooms
                .Select(br => br.CheckInDate)
                .ToList();
            var unavailableDates = bookedDates.Select(date => date.ToString("yyyy-MM-dd")).ToList();
            return unavailableDates;
        }


        // GET: BookRooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BookRooms == null)
            {
                return NotFound();
            }

            var bookRooms = await _context.BookRooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookRooms == null)
            {
                return NotFound();
            }

            return View(bookRooms);
        }

        // GET: BookRooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookRooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomId,CustomerEmail,OwnerEmail,BookingDate,CheckInDate,CheckOutDate")] BookRooms bookRooms)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookRooms);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookRooms);
        }

        // GET: BookRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BookRooms == null)
            {
                return NotFound();
            }

            var bookRooms = await _context.BookRooms.FindAsync(id);
            if (bookRooms == null)
            {
                return NotFound();
            }
            return View(bookRooms);
        }

        // POST: BookRooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,CustomerEmail,OwnerEmail,BookingDate,CheckInDate,CheckOutDate")] BookRooms bookRooms)
        {
            if (id != bookRooms.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookRooms);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookRoomsExists(bookRooms.Id))
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
            return View(bookRooms);
        }

        // GET: BookRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BookRooms == null)
            {
                return NotFound();
            }

            var bookRooms = await _context.BookRooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookRooms == null)
            {
                return NotFound();
            }

            return View(bookRooms);
        }

        // POST: BookRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BookRooms == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BookRooms'  is null.");
            }
            var bookRooms = await _context.BookRooms.FindAsync(id);
            if (bookRooms != null)
            {
                _context.BookRooms.Remove(bookRooms);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookRoomsExists(int id)
        {
            return (_context.BookRooms?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookNow(BookRooms bookingDetails)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null || !User.Identity.IsAuthenticated)
                {
                    _logger.LogError("User not authenticated");
                    return NotFound("User not authenticated");
                }

                bookingDetails.CustomerEmail = currentUser.Email;
                bookingDetails.BookingDate = DateTime.Now;
                var isRoomAvailable = await IsRoomAvailable(bookingDetails.RoomsId, bookingDetails.CheckInDate, bookingDetails.CheckOutDate, bookingDetails);

                if (!isRoomAvailable)
                {
                    ModelState.AddModelError("RoomsId", "Selected room is not available for the specified date range");
                    ModelState.AddModelError("CheckInDate", "Selected room is not available for the specified date range");
                    ModelState.AddModelError("CheckOutDate", "Selected room is not available for the specified date range");

                    ViewBag.Rooms = new SelectList(await _context.Rooms.ToListAsync(), "Id", "Name");
                    return View("Index", bookingDetails); 
                }

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == bookingDetails.RoomsId);

                if (room == null)
                {
                    ViewBag.Rooms = new SelectList(await _context.Rooms.ToListAsync(), "Id", "Name");
                    return View("Index", bookingDetails); 
                }

                bookingDetails.Rooms = room;
                bookingDetails.OwnerEmail = room.OwnerEmail;
                bookingDetails.Price = room.Price;

                _context.BookRooms.Add(bookingDetails);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Rooms", new { id = room.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        private async Task<bool> IsRoomAvailable(int roomId, DateTime checkInDate, DateTime checkOutDate, BookRooms bookingDetails)
        {
            var overlappingBookings = await _context.BookRooms
                .Where(b => b.RoomsId == roomId && b.Id != bookingDetails.Id && (checkOutDate > b.CheckInDate && checkInDate < b.CheckOutDate))
                .ToListAsync();
            return overlappingBookings.Count == 0;
        }
    }
}





