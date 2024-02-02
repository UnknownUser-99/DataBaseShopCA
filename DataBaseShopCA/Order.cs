using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseShopCA
{
    class Order
    {
        public string Number { get; set; }
        public string Date { get; set; }
        public string Sum { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public List<Product> Products { get; } = new List<Product>();
    }
}
