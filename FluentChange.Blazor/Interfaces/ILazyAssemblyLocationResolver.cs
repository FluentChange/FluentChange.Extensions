namespace FluentChange.Blazor.Interfaces
{
    public interface ILazyAssemblyLocationResolver
    {   
        string Resolve(string name);
    }
}
