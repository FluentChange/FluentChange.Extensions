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

            var t1 = new Event();
            t1.Id = Guid.Parse("33bc54bf-9e0c-494d-84ad-cc239837f543");
            t1.Title = "Test Event 1";
            t1.Description = "Description Event 1";
            t1.Date = DateTime.Now;

            var t2 = new Event();
            t2.Id = Guid.Parse("44bc54bf-9e0c-494d-84ad-cc239837f543");
            t2.Title = "Test Event 2";
            t2.Description = "Description Event 2";
            t2.Date = DateTime.Now.AddDays(1);

            events.Add(t1);
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
