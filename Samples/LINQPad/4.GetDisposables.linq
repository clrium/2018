<Query Kind="Program">
  <NuGetReference>ClrMD.Extensions</NuGetReference>
  <Namespace>ClrMD.Extensions</Namespace>
</Query>

void Main()
{
    using (var session = ClrMDSession.AttachToProcess("ConsoleApplication"))
    {
        session.ShowDisposables();
        ;
    }
}

public static class LocalExtensions
{
    public static void ShowDisposables(this ClrMDSession session)
    {
        (   // Start with all objects
            from obj in session.AllObjects
            where obj.Type.Interfaces.Any(i => i.Name.Contains("IDisposable"))
            select new {
                obj.TypeName,
                RefBy = obj.EnumerateReferenceBy()
            }
        ).Take(100).ToList().Dump("Heap statistics", depth: 2);
    }
}