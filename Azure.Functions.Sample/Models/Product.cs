using System;
using System.Collections.Generic;
using System.Text;

namespace DemoCRUDLFunctions.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
    }

    public class ApiProduct
    {
        public Guid Identifier { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public double Price { get; set; }
    }
}
