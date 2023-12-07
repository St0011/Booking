using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class Rooms
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Please enter the City")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Please enter the RoomType")]
        public string? RoomType { get; set; }

        [Required(ErrorMessage = "Please enter the Price")]
        public int Price { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }
        public string? OwnerEmail { get; set; }

        [NotMapped]
        public IFormFile? CoverPhoto { get; set; }
        public string CoverPhotoFileName { get; set; } = "";
        public virtual List<BookRooms>? BookRooms { get; set; }
    }
}