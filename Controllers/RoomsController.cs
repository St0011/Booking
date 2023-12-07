using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Booking.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager,
            ILogger<RoomsController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _logger = logger;
        }
        public IActionResult Index(string sortOrder)
        {
            try
            {
                ViewBag.PriceSortParam = String.IsNullOrEmpty(sortOrder) ? "regular" : sortOrder;

                var rooms = _context.Rooms.AsQueryable();

                switch (sortOrder)
                {
                    case "price_desc":
                        rooms = rooms.OrderByDescending(r => r.Price);
                        break;
                    case "price_asc":
                        rooms = rooms.OrderBy(r => r.Price);
                        break;
                    default:
                        rooms = rooms.OrderBy(r => r.Name); 
                        break;
                }

                return View(rooms.ToList());
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }




        //CREATE
        [Authorize]
        public IActionResult Create()
        {
            var room = new Rooms
            {
              
            };
            return View(room);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,City,RoomType,Price,Description,OwnerEmail,CoverPhoto,RoomsId")]
        Rooms rooms)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                rooms.OwnerEmail = currentUser.Email;

                if (rooms.CoverPhoto != null && rooms.CoverPhoto.Length > 0)
                {
                    string folder = "uploads/cover";
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(rooms.CoverPhoto.FileName);
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, folder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await rooms.CoverPhoto.CopyToAsync(stream);
                    }

                    rooms.CoverPhotoFileName = uniqueFileName;
                    _context.Add(rooms);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            return View(rooms);
        }


        //EDIT
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }

            var rooms = await _context.Rooms.FindAsync(id);
            if (rooms == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            Console.WriteLine($"Current User: {currentUser?.Email}, Room Owner: {rooms?.OwnerEmail}");
            if (currentUser == null || (!User.IsInRole("Admin") && rooms?.OwnerEmail != currentUser.Email))
            {
                return Forbid();
            }

            return View(rooms);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,City,RoomType,Price,Description")] Rooms rooms, IFormFile newCoverPhoto)
        {
            var existingRoom = await _context.Rooms.FindAsync(id);

            if (existingRoom == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || existingRoom.OwnerEmail != currentUser.Email)
            {
                return Forbid();
            }

            existingRoom.Name = rooms.Name;
            existingRoom.City = rooms.City;
            existingRoom.RoomType = rooms.RoomType;
            existingRoom.Price = rooms.Price;
            existingRoom.Description = rooms.Description;

            try
            {
                if (newCoverPhoto != null && newCoverPhoto.Length > 0)
                {
                    string folder = "uploads/cover";
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(newCoverPhoto.FileName);
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, folder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await newCoverPhoto.CopyToAsync(stream);
                    }

                    existingRoom.CoverPhotoFileName = uniqueFileName;
                }

                _context.Update(existingRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return View(existingRoom);
            }
        }

        //DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }

            var rooms = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rooms == null)
            {
                return NotFound();
            }

            return View(rooms);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }

            var rooms = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rooms == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || (!User.IsInRole("Admin") && rooms.OwnerEmail != currentUser.Email))
            {
                return Forbid();
            }

            return View(rooms);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Rooms == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Rooms' is null.");
            }

            var rooms = await _context.Rooms.FindAsync(id);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || (!User.IsInRole("Admin") && rooms.OwnerEmail != currentUser.Email))
            {
                return Forbid();
            }

            if (rooms != null)
            {
                _context.Rooms.Remove(rooms);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool RoomsExists(int id)
        {
            return (_context.Rooms?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
