using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using ClrMD.Extensions;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;
using Microsoft.Diagnostics.Runtime;

namespace DataContextDriverDemo.Astoria
{
    static class SchemaBuilder
    {
        public static List<ExplorerItem> GetAppDomainsList(ClrMDSession session, HashSet<ClrType> types)
        {
            return session.Runtime.AppDomains.Select(domain => BuildAppDomainExplorerItem(domain, types)).ToList();
        }

        private static ExplorerItem BuildAppDomainExplorerItem(ClrAppDomain domain, HashSet<ClrType> types)
        {
            var name = string.IsNullOrEmpty(domain.Name) ? "(noname)" : domain.Name;
            var item = new ExplorerItem(name, ExplorerItemKind.Category, ExplorerIcon.Schema);
            item.Children = new List<ExplorerItem>();
            foreach (var module in domain.Modules)
            {
                item.Children.Add(BuildModuleExplorerItem(module, types));
            }
            return item;
        }

        private static ExplorerItem BuildModuleExplorerItem(ClrModule module, HashSet<ClrType> types)
        {
            var name = string.IsNullOrEmpty(module.Name) ? "(noname)" : module.Name;
            var item = new ExplorerItem(name, ExplorerItemKind.Category, ExplorerIcon.OneToMany)
            {
                ToolTipText = $"{module.Name} ({module.AssemblyName})",
                Children = new List<ExplorerItem>()
            };
            
            foreach (var type in module.EnumerateTypes().Where(types.Contains))
            {
                item.Children.Add(BuildExplorerItem(type));
            }
            return item;
        }

        private static ExplorerItem BuildExplorerItem(ClrType type)
        {
            var item = new ExplorerItem(type.Name, ExplorerItemKind.CollectionLink, ExplorerIcon.Table)
            {
                Tag = type,
                Kind = ExplorerItemKind.QueryableObject,
                DragText = $"Session.EnumerateDynamicObjects(\"{type.Name}\").Take(100)"
            };
            return item;
        }
    }
}
