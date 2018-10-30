<Query Kind="Program">
  <NuGetReference>ClrMD.Extensions</NuGetReference>
  <NuGetReference>System.Runtime.CompilerServices.Unsafe</NuGetReference>
  <Namespace>ClrMD.Extensions</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
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
        Dictionary<string, int> strings = new Dictionary<string?, int>();
        var objects = session.EnumerateDynamicObjects("System.String");
        foreach (var obj in objects)
        {            
            string s = obj.Type.GetValue(obj.Address) as string;

            if (!strings.ContainsKey(s))
            {
                strings[s] = 0;
            }

            strings[s] = strings[s] + 1;
        }
        
        // Display
        strings
            .Where(pair => pair.Value > 1)
            .Select(pair => new { String = pair.Key.Replace("\n", "## ").Replace("\r", " ##"), pair.Value})
            .Dump();
    }
}