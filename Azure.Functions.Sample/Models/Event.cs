using FluentChange.Extensions.Common.Rest;
using System;

namespace DemoCRUDLFunctions.Models
{
    public class Event: IEntityWithId
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
