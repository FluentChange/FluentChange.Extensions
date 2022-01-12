using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using FluentChange.Extensions.Azure.Functions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoCRUDLFunctions.Services
{
    public class ProductService : ICRUDLServiceWithId<Product>
    {
        private List<Product> products;
        public ProductService()
        {
            products = new List<Product>();

            var p1 = new Product();
            p1.Id = Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543");
            p1.Title = "Test Product 1";
            p1.Description = "Description Product 1";
            p1.Price = 123.5;

            var p2 = new Product();
            p2.Id = Guid.Parse("77bc54bf-9e0c-494d-84ad-cc239837f543");
            p2.Title = "Test Product 2";
            p2.Description = "Description Product 2";
            p2.Price = 444.4;

            products.Add(p1);
            products.Add(p2);
        }

        public Product Create(Product product)
        {
            if (product.Id != Guid.Empty) throw new ArgumentException();
            product.Id = Guid.NewGuid();
            products.Add(product);
            return product;
        }
        public Product Read(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            return products.SingleOrDefault(t => t.Id == id);
        }
        public Product Update(Product product)
        {
            if (product.Id == Guid.Empty) throw new ArgumentException();
            Delete(product.Id);
            products.Add(product);
            return product;
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
