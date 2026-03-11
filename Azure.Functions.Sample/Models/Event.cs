#nullable enable
using FluentChange.Extensions.Common.Models;
using FluentChange.Extensions.Common.Rest;
using System;

namespace DemoCRUDLFunctions.Models
{
    public class Event: IEntityWithId
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime Date { get; set; }
    }
}
