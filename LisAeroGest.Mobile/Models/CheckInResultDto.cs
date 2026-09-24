using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisAeroGest.Mobile.Models
{
    public class CheckInResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int BoardingPassId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public string QRData { get; set; } = string.Empty;

    }
}
