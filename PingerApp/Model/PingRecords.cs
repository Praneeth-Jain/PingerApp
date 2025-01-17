using System;
using System.ComponentModel.DataAnnotations;

namespace PingerApp.Model
{
    public class PingRecord
    {
        [Key]
        public int Id { get; set; }
        public string IPAddress { get; set; }
        public string Status { get; set; }
        public long Rtt { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
