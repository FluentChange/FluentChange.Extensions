using FluentChange.Blazor.WebAssembly;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentChange.Blazor.WebAssembly
{
    public class Lazy : ComponentBase
    {
        [Inject]
        public LazyLoaderService AssemblyLoader { get; set; }

        private Type type;

        private Assembly loaded;
        public IComponent? Instance { get; private set; } = null;

        [Parameter] public string Plugin { get; set; }
        [Parameter] public string Component { get; set; }

        [Parameter] public IEnumerable<KeyValuePair<string, object>> Parameters { get; set; } = new Dictionary<string, object>();


        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);
            if (Component == null)
            {
                throw new InvalidOperationException($"The {nameof(Lazy)} component requires a value for the parameter {nameof(Component)}.");
            }
        }


        //protected override void OnInitialized()
        //{
        //    base.OnInitialized(); // trigger initial render

        //}

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(false);

            loaded = await AssemblyLoader.LoadAssembliesFromWeb(Plugin);

            type = loaded.GetTypes().First(t => t.Name == Component);
                      

            StateHasChanged(); // always re-render after load

        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (loaded == null)
            {
            
                builder.OpenElement(1, "p");
                builder.AddContent(2, "Loading....");              
                builder.CloseElement();
                return;
            }
                    

 
            builder.OpenComponent(0, type);
            builder.AddMultipleAttributes(0, Parameters);
            builder.AddComponentReferenceCapture(1, (componentRef) =>
            {
                Instance = (IComponent)componentRef;

            });
            builder.CloseComponent();
        }
    }
}
