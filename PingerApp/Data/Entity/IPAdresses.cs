using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingerApp.Data.Entity
{
    public class IPAdresses
    {
        [Key]
        public int ID { get; set; }
        public string IPAddress { get; set; }
    }
}
