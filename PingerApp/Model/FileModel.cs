using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingerApp.Model
{
    public class FileModel
    {
        public string Address { get; set; }

        public string Status { get; set; }

        public long Rtt { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;
    }
}
