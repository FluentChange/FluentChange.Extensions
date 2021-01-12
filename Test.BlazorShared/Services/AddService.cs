using FluentChange.Blazor.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Blazor.Shared.Services
{
    public class AddService : IScopedService
    {
        public AddService()
        {
            Console.WriteLine("CTOR Service 1");
        }

        public int Add(int a, int b)
        {
            return a + b;
        }

    }
}
