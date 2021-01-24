using Blazored.LocalStorage;
using FluentChange.Extensions.Blazor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Blazor.Services
{
    public class TestLocalStorage: ISingletonService
    {
        private readonly ISyncLocalStorageService localStorage;
        public TestLocalStorage(ISyncLocalStorageService localStorage)
        {
            this.localStorage = localStorage;
        }

        public void Write(string text)
        {
            localStorage.SetItem("TEXT", text);
        }

        public string Read()
        {
            var text = localStorage.GetItem<string>("TEXT");
            return text;
        }
    }
}
