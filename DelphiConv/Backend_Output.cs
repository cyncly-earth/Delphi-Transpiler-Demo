// --- BACKEND OUTPUT (C# API & DTOs) ---
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HotelSystem.Backend
{
    public class CalendarItemDto
    {
        [Key]
        public int BookID { get; set; }
        public int RentID { get; set; }
        public int AccomID { get; set; }
        public int ClientID { get; set; }
        public string Status { get; set; }
        public decimal Outstanding { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AccomName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Occupants { get; set; }
        public string TravelInfo { get; set; }
    }

    public interface ICalendarService
    {
        Task ItemClickAsync(int BookID);
        Task UpdateCalendarAsync(DateTime StartDate, DateTime EndDate);
        Task EmailBitmapAsync(string Image, string Email, string EmailUser, string Body);
    }
}
