<Query Kind="Program">
  <NuGetReference>ClrMD.Extensions</NuGetReference>
  <Namespace>ClrMD.Extensions</Namespace>
</Query>

void Main()
{
    using (var session = ClrMDSession.AttachToProcess("ConsoleApplication"))
    {
        session.Run();
        ;
    }
}

public static class LocalExtensions
{    
    public static void Run(this ClrMDSession session)
    {
        (   // Start with all objects
            from o in session.AllObjects 
            // Group by object type.
            group o by o.Type into typeGroup
            // Get the instance count of this type.
            let count = typeGroup.Count()
            // Get the memory usage of all instances of this type
            let totalSize = typeGroup.Sum(item => (double)item.Size)
            // Orderby to get objects with most instance count first
            orderby count descending
            select new
            {
                Type = typeGroup.Key.Name,
                Count = count,
                TotalSizeMB = (totalSize / 1024 / 1024),
                // Get the first 100 instances of the type.
                First100Objects = typeGroup.Take(100),
            }
        ).Take(100).Dump("Heap statistics", depth:0);
    }
}