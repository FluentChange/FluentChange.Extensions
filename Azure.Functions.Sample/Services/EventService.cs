using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoCRUDLFunctions.Services
{
    public class EventService 
    {
        private List<Event> events;
        public EventService()
        {
            events = new List<Event>();

            var t = new Event();
            t.Id = Guid.Parse("33bc54bf-9e0c-494d-84ad-cc239837f543");
            t.Title = "Test Event";
            t.Description = "Description Event";
            t.Date = DateTime.Now;

            events.Add(t);
        }

        public Event New(Event @event)
        {
            if (@event.Id != Guid.Empty) throw new ArgumentException();
            @event.Id = Guid.NewGuid();
            events.Add(@event);
            return @event;
        }
        public Event Get(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            return events.SingleOrDefault(t => t.Id == id);
        }
        public Event Edit(Event @event)
        {
            if (@event.Id == Guid.Empty) throw new ArgumentException();
            Remove(@event.Id);
            events.Add(@event);
            return @event;
        }
        public void Remove(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            var existignEvent = Get(id);
            if (existignEvent == null) throw new NullReferenceException();
            events.Remove(existignEvent);
        }
        public IEnumerable<Event> All()
        {
            return events;
        }
    }
}
