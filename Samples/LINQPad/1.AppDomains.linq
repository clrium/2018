<Query Kind="Program">
  <NuGetReference>ClrMD.Extensions</NuGetReference>
  <Namespace>ClrMD.Extensions</Namespace>
</Query>

void Main()
{
    using (var session = ClrMDSession.AttachToProcess("ConsoleApplication"))
    {
        session.Run();
    }
}
public static class LocalExtensions
{
    public static void Run(this ClrMDSession session)
    {
        (   // Start with all objects
            from appDomain in session.Runtime.AppDomains
            select new
            {
                appDomain.Name,
                Modules = appDomain.Modules.Select(m=> m.AssemblyName)
            }
        ).Dump();
    }
}