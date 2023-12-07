using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    public class BookRooms
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("RoomsId")]
        [Display(Name = "Select the specific Room")]
        public int RoomsId { get; set; }
        public string? CustomerEmail { get; set; }
        public string? OwnerEmail { get; set; }
        public int Price { get; set; }
        public DateTime BookingDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please enter the Check-In Date")]
        public DateTime CheckInDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please enter the Check-Out Date")]
        public DateTime CheckOutDate { get; set; } = DateTime.Today;
        public virtual Rooms? Rooms { get; set; }
    }
}