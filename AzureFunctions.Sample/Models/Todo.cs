using System;
using System.Collections.Generic;
using System.Text;

namespace DemoCRUDLFunctions.Models
{
    public class Todo
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
    }
}
