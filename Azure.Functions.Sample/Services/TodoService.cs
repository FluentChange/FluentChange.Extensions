using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoCRUDLFunctions.Services
{
    public class TodoService : ICRUDLService<Todo>
    {
        private List<Todo> todos;
        public TodoService()
        {
            todos = new List<Todo>();

            var t = new Todo();
            t.Id = Guid.Parse("16bc54bf-9e0c-494d-84ad-cc239837f543");
            t.Title = "Test Todo";
            t.Description = "Description Todo";
            t.Done = false;

            todos.Add(t);
        }

        public Todo Create(Todo todo)
        {
            if (todo.Id != Guid.Empty) throw new ArgumentException();
            todo.Id = Guid.NewGuid();
            todos.Add(todo);
            return todo;
        }
        public Todo Read(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            return todos.SingleOrDefault(t => t.Id == id);
        }
        public Todo Update(Todo todo)
        {
            if (todo.Id == Guid.Empty) throw new ArgumentException();
            Delete(todo.Id);
            todos.Add(todo);
            return todo;
        }
        public void Delete(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            var existingTodo = Read(id);
            if (existingTodo == null) throw new NullReferenceException();
            todos.Remove(existingTodo);
        }
        public IEnumerable<Todo> List()
        {
            return todos;
        }
    }
}
