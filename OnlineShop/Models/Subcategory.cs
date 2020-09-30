using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IShop.Models
{
    public class Subcategory
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long CategoryId { get; set; }

        public Category Category { get; set; }

        public List<Good> goods { get; set; }

        public Subcategory()
        {
            goods = new List<Good>();
        }
    }
}
