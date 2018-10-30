using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using ClrMD.Extensions;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;
using Microsoft.Diagnostics.Runtime;

namespace DataContextDriverDemo.Astoria
{
    public class AstoriaDynamicDriver : DynamicDataContextDriver
    {
        private const string DataContextTemplate =
        @"  namespace LINQPad.Extension
            {
                using System;
                using System.Collections.Generic;
                using ClrMD.Extensions;
                [usings]

                public class ClrMDWrapper
                {
                    ClrMDSession _session;

                    public ClrMDWrapper(ClrMDSession session)
                    {
                        _session = session;
                    }

                    public ClrMDSession Session
                    {
                        get { return _session; }
                    }

                    [properties]
                }
            }";

        private static readonly LocalDataStoreSlot _threadStorageSlot;

        public override string Name => "ClrMD Connection";

        public override string Author => "Stanislav Sidristy";

        static AstoriaDynamicDriver()
        {
            _threadStorageSlot = Thread.AllocateDataSlot();
        }

        public override string GetConnectionDescription (IConnectionInfo cxInfo)
        {
            return $"Process: {new ConnectionProperties (cxInfo).ProcessName}";
        }

        public override ParameterDescriptor [] GetContextConstructorParameters (IConnectionInfo cxInfo)
        {
            return new [] { new ParameterDescriptor ("session", typeof(ClrMDSession).FullName) };
        }

        public override object [] GetContextConstructorArguments (IConnectionInfo cxInfo)
        {
            var props = new ConnectionProperties(cxInfo);
            var session = ClrMDSession.AttachToProcess(props.ProcessName);

            Thread.SetData(_threadStorageSlot, session);

            return new object [] { session };
        }

        public override void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            var session = (ClrMDSession)context
                .GetType()
                .GetProperty("Session")
                .GetValue(context);

            session?.Dispose();
        }

        public override IEnumerable<string> GetAssembliesToAdd (IConnectionInfo cxInfo)
        {
            var assemblies = new List<Assembly>
            {
                typeof(ClrMDSession).Assembly,
                typeof(ClrType).Assembly
            };
            return assemblies.Select(a => a.Location).ToList();
        }

        public override IEnumerable<string> GetNamespacesToAdd (IConnectionInfo cxInfo)
        {
            return new []
            {
                "ClrMD.Extensions"
            };
        }

        public override bool AreRepositoriesEquivalent (IConnectionInfo r1, IConnectionInfo r2)
        {
            // Two repositories point to the same endpoint if their URIs are the same.
            return object.Equals (r1.DriverData.Element ("ProcessName"), r2.DriverData.Element ("ProcessName"));
        }

        public override bool ShowConnectionDialog (IConnectionInfo cxInfo, bool isNewConnection)
        {
            // Populate the default URI with a demo value:
            if (isNewConnection) new ConnectionProperties (cxInfo).ProcessName = "ConnectionName";

            var result = new ConnectionDialog (cxInfo).ShowDialog ();
            return result == true;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly (IConnectionInfo cxInfo, AssemblyName assemblyToBuild,
            ref string nameSpace, ref string typeName)
        {
            var props = new ConnectionProperties(cxInfo);
            nameSpace = "LINQPad.Extension";
            typeName = "ClrMDWrapper";

            var allGeneratedSources = new List<string>();
            var sbContextUsings = new StringBuilder();
            var sbContextProperties = new StringBuilder();

            using (var session = ClrMDSession.AttachToProcess(props.ProcessName))
            {
                var hashset = new HashSet<ClrType>();
                foreach (var sessionAllObject in session.AllObjects)
                {
                    hashset.Add(sessionAllObject.Type);
                }


                CompilerResults results;
                string outputName = assemblyToBuild.CodeBase;
                using (var codeProvider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } }))
                {
                    string[] assemblies = GetAssembliesToAdd(cxInfo).ToArray();

                    var compilerOptions = new CompilerParameters(assemblies, outputName, true);
                    var dataContext = DataContextTemplate
                        .Replace("[usings]", sbContextUsings.ToString())
                        .Replace("[properties]", sbContextProperties.ToString());

                    allGeneratedSources.Add(dataContext);
                    results = codeProvider.CompileAssemblyFromSource(compilerOptions, allGeneratedSources.ToArray());
                    if (results.Errors.Count > 0)
                    {
                        var sbErr = new StringBuilder();

                        foreach (object o in results.Errors)
                        {
                            sbErr.AppendLine(o.ToString());
                        }

                        // Is there any better troubleshooting mechanism? 
                        MessageBox.Show(sbErr.ToString(), "Error compiling generated code");
                    }
                }

                return SchemaBuilder.GetAppDomainsList(session, hashset);
            }
        }
    }
}
