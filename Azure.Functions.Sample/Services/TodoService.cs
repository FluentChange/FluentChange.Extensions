using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using FluentChange.Extensions.Azure.Functions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoCRUDLFunctions.Services
{
    public class TodoService : ICRUDLServiceWithId<Todo>
    {
        private List<Todo> todos;
        public TodoService()
        {
            todos = new List<Todo>();

            var t1 = new Todo();
            t1.Id = Guid.Parse("11bc54bf-9e0c-494d-84ad-cc239837f543");
            t1.Title = "Test Todo 1";
            t1.Description = "Description Todo 1";
            t1.Done = false;

            var t2 = new Todo();
            t2.Id = Guid.Parse("22bc54bf-9e0c-494d-84ad-cc239837f543");
            t2.Title = "Test Todo 2";
            t2.Description = "Description Todo 2";
            t2.Done = false;

            todos.Add(t1);
            todos.Add(t2);
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
