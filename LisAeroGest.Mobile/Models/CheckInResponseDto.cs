using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisAeroGest.Mobile.Models
{
    public class CheckInResponseDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public DateTime IssuedAt { get; set; }
        public string QRData { get; set; } = string.Empty;
    }
}
