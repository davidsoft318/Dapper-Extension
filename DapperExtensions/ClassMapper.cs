﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions
{
    public interface IClassMapper
    {
        string SchemaName { get; }
        string TableName { get; }
        IList<IPropertyMap> Properties { get; }
    }

    public class ClassMapper : IClassMapper
    {
        public string SchemaName { get; private set; }
        public string TableName { get; private set; }
        public IList<IPropertyMap> Properties { get; private set; }

        public ClassMapper()
        {
            Properties = new List<IPropertyMap>();
        }

        public virtual void Schema(string schemaName)
        {
            SchemaName = schemaName;
        }

        public virtual void Table(string tableName)
        {
            TableName = tableName;
        }
    }

    public interface IClassMapper<T> where T : class
    {
        PropertyMap Map(Expression<Func<T, object>> expression);
        PropertyMap Map(PropertyInfo propertyInfo);
    }

    public class ClassMapper<T> : ClassMapper, IClassMapper<T> where T : class
    {
        public ClassMapper()
        {
            Table(typeof(T).Name);
        }

        public PropertyMap Map(Expression<Func<T, object>> expression)
        {
            PropertyInfo propertyInfo = GetProperty(expression) as PropertyInfo;
            return Map(propertyInfo);
        }

        public PropertyMap Map(PropertyInfo propertyInfo)
        {
            PropertyMap result = new PropertyMap(propertyInfo);
            Properties.Add(result);
            return result;
        }

        protected MemberInfo GetProperty(LambdaExpression lambda)
        {
            Expression expr = lambda;
            for (; ; )
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;
                        break;
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression)expr;
                        MemberInfo mi = memberExpression.Member;
                        return mi;
                    default:
                        return null;
                }
            }
        }
    }

    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper()
        {
            Type type = typeof(T);
            Table(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                PropertyMap map = Map(propertyInfo);
                if (map.PropertyInfo.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (map.PropertyInfo.PropertyType == typeof(int))
                    {
                        map.Key(KeyType.Identity);
                    }
                    else if (map.PropertyInfo.PropertyType == typeof(Guid))
                    {
                        map.Key(KeyType.Guid);
                    }
                    else
                    {
                        map.Key(KeyType.Assigned);
                    }
                }
            }
        }
    }

    public class PlurizedAutoClassMapper<T> : AutoClassMapper<T> where T : class
    {
        public override void Table(string tableName)
        {
            base.Table(tableName + "s");
        }
    }
}