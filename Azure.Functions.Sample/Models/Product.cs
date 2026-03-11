#nullable enable
using FluentChange.Extensions.Common.Models;
using FluentChange.Extensions.Common.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoCRUDLFunctions.Models
{
    public class Product: IEntityWithId
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public double Price { get; set; }
    }

    public class ApiProduct : IEntityWithId
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Text { get; set; } = default!;
        public double Price { get; set; }
    }
}
