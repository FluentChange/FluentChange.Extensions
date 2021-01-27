using FluentChange.Extensions.Common.Rest;
using System;
using System.Collections.Generic;
using System.Text;

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
