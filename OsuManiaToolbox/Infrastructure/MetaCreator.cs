using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace OsuManiaToolbox.Infrastructure;

public class MetaCreator
{
    public static DataTable Create<T>() where T : class
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Type", typeof(string));
        table.Columns.Add("Comment", typeof(string));

        foreach (var method in typeof(T).GetMethods())
        {
            if (TryGetDescription(method, out var description))
            {
                var name = $"{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.Name))})";
                table.Rows.Add(name, method.ReturnType.Name, description);
            }
        }
        foreach (var prop in typeof(T).GetProperties())
        {
            if (TryGetDescription(prop, out var description))
            {
                var typeName = prop.PropertyType.IsGenericType
                    ? $"{prop.PropertyType.Name}<{string.Join(", ", prop.PropertyType.GenericTypeArguments.Select(t => t.Name))}>"
                    : prop.PropertyType.Name;
                table.Rows.Add(prop.Name, typeName, description);
            }
        }
        return table;
    }

    private static bool TryGetDescription(MemberInfo info, out string description)
    {
        var attr = info.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
        if (attr != null)
        {
            description = attr.Description;
            return true;
        }
        description = string.Empty;
        return false;
    }
}
