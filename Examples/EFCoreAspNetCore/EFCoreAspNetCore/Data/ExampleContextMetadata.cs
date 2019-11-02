using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreAspNetCore.Data
{
    public static class ExampleContextMetadata
    {
        public static Dictionary<Type, Dictionary<string, ExampleContextColumnDefinition>> Tables { get; } = new Dictionary<Type, Dictionary<string, ExampleContextColumnDefinition>>();

        public static void Build(ExampleContext context)
        {
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var columns = new Dictionary<string, ExampleContextColumnDefinition>();
                foreach (var column in entityType.GetProperties())
                {
                    columns.Add(column.Name, new ExampleContextColumnDefinition
                    {
                        ClrType = column.ClrType,
                        Nullable = column.IsNullable,
                        MaxLength = column.ClrType == typeof(string) ? column.FindAnnotation("MaxLength")?.Value as int? : null
                    });
                }

                Tables.Add(entityType.ClrType, columns);
            }
        }
    }

    public class ExampleContextColumnDefinition
    {
        public Type ClrType { get; set; }
        public bool Nullable { get; set; }
        public int? MaxLength { get; set; }
    }
}
