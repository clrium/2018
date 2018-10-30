<Query Kind="Program">
  <NuGetReference>ClrMD.Extensions</NuGetReference>
  <Namespace>ClrMD.Extensions</Namespace>
</Query>

void Main()
{
    using (var session = ClrMDSession.AttachToProcess("PerfWatson2"))
    {
        session.Run();
        ;
    }
}

public static class LocalExtensions
{
    public static void Run(this ClrMDSession session, bool includeThreadName = true)
    {
        // Get the thread names from the 'Thread' instances of the heap.
        var threadsInfo = from o in session.EnumerateDynamicObjects("System.Threading.Thread")
                          select new
                          {
                              ManagedThreadId = (int)o.Dynamic.m_ManagedThreadId,
                              Name = (string)o.Dynamic.m_Name
                          };
        
        (   // Join the ClrThreads with their respective thread names
            from t in session.Runtime.Threads
            join info in threadsInfo on t.ManagedThreadId equals info.ManagedThreadId into infoGroup
            let name = infoGroup.Select(item => item.Name).FirstOrDefault() ?? ""
            select new
                {
                    ManagedThreadId = t.ManagedThreadId,
                    Name = name,
                    StackTrace = t.GetStackTrace(),
                    Exception = t.CurrentException,
                    t.BlockingObjects
                }
        ).ToList().Dump("Threads", depth:1);
    }
}