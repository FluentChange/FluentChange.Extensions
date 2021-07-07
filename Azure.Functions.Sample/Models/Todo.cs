using FluentChange.Extensions.Common.Models;
using System;

namespace DemoCRUDLFunctions.Models
{
    public class Todo: IEntityWithId
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
    }
}
