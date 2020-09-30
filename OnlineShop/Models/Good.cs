using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IShop.Models
{
    public class Good
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public double Price { get; set; }

        public int CountInStorage { get; set; }

        public byte[] Image { get; set; }

        public int Discount { get; set; }

        public string Note { get; set; }

        public string Maker { get; set; }

        public string Guarantee { get; set; }

        public long SubcategoryId { get; set; }

        public Subcategory Subcategory { get; set; }
    }
}
