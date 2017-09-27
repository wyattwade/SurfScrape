using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapingExample
{

    public class Surfboard
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public int? Height { get; set; }
        public decimal Width { get; set; }
        public decimal Volume { get; set; }
        public string Link { get; set; }
        public int? Price { get; set; }
        public string Image { get; set; }

        public bool FromInternalUser { get; set; }
        
    }
}






