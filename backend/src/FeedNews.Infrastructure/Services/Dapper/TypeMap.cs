using System.Reflection;
using Dapper;

namespace FeedNews.Infrastructure.Services.Dapper;

public class TypeMap<T> : SqlMapper.ITypeMap
{
    private readonly ColumnTypeMapper<T> defaultTypeMap = new();

    public ConstructorInfo? FindConstructor(string[] names, Type[] types)
    {
        return defaultTypeMap.FindConstructor(names, types);
    }

    public ConstructorInfo? FindExplicitConstructor()
    {
        return defaultTypeMap.FindExplicitConstructor();
    }

    public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName)
    {
        return defaultTypeMap.GetConstructorParameter(constructor, columnName);
    }

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        List<SqlMapper.ITypeMap> fallbackMappers = new();
        fallbackMappers.Add(defaultTypeMap);

        FallbackTypeMapper fallbackMapper = new(fallbackMappers);

        SqlMapper.IMemberMap? member = fallbackMapper.GetMember(columnName);
        if (member == null) throw new Exception($"Column {columnName} cannot map to an object");

        return member;
    }
}