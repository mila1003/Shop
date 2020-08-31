using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Subcategory> subcategories { get; set; }

        public Category()
        {
            subcategories = new List<Subcategory>();
        }
    }
}
