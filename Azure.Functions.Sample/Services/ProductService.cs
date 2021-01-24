using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoCRUDLFunctions.Services
{
    public class ProductService : ICRUDLService<Product>
    {
        private List<Product> products;
        public ProductService()
        {
            products = new List<Product>();

            var t = new Product();
            t.Id = Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543");
            t.Title = "Test Product";
            t.Description = "Description Product";
            t.Price = 123.5;

            products.Add(t);
        }

        public void Create(Product todo)
        {
            if (todo.Id != Guid.Empty) throw new ArgumentException();
            products.Add(todo);
        }
        public Product Read(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            return products.SingleOrDefault(t => t.Id == id);
        }
        public void Update(Product todo)
        {
            if (todo.Id == Guid.Empty) throw new ArgumentException();
            Delete(todo.Id);
            products.Add(todo);
        }
        public void Delete(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            var existingTodo = Read(id);
            if (existingTodo == null) throw new NullReferenceException();
            products.Remove(existingTodo);
        }
        public IEnumerable<Product> List()
        {
            return products;
        }
    }
}
