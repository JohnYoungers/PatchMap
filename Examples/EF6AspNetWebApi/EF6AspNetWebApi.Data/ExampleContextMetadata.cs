using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Data
{
    public static class ExampleContextMetadata
    {
        public static Dictionary<Type, Dictionary<string, ExampleContextColumnDefinition>> Tables { get; } = new Dictionary<Type, Dictionary<string, ExampleContextColumnDefinition>>();

        public static void Build(ExampleContext context)
        {
            var tables = (context as IObjectContextAdapter).ObjectContext.MetadataWorkspace.GetItems(DataSpace.CSpace).Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType).Cast<EntityType>().ToList();
            foreach (var i in tables)
            {
                var tableType = i.MetadataProperties.First(p => p.Name.Contains("ClrType")).Value as Type;

                var columns = new Dictionary<string, ExampleContextColumnDefinition>();
                foreach (var p in i.Properties)
                {
                    columns.Add(p.Name, new ExampleContextColumnDefinition
                    {
                        Nullable = p.Nullable,
                        MaxLength = p.MaxLength
                    });
                }
                foreach (var n in i.NavigationProperties)
                {
                    columns.Add(n.Name, new ExampleContextColumnDefinition
                    {
                        Nullable = n.TypeUsage.Facets.Any(f => f.Name == "Nullable" && (bool)f.Value)
                    });
                }

                Tables.Add(tableType, columns);
            }
        }
    }

    public class ExampleContextColumnDefinition
    {
        public bool Nullable { get; set; }
        public int? MaxLength { get; set; }
    }
}
