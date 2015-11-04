using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;

namespace Scheduler.Extensions
{
	public static class CollectionExtensions
	{
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> array, Action<T> action)
		{
			foreach (var i in array)
				action(i);
			return array;
		}

		public static IEnumerable<T> ForEach<T>(this IEnumerable array, Action<T> action)
		{
			return array.Cast<T>().ForEach<T>(action);
		}

		public static IEnumerable<RT> ForEach<T, RT>(this IEnumerable<T> array, Func<T, RT> func)
		{
			var list = new List<RT>();
			foreach (var i in array)
			{
				var obj = func(i);
				if (obj != null)
					list.Add(obj);
			}
			return list;
		}

		public static DataTable ToDataTable<T>(this IEnumerable<T> array)
		{
			DataTable table = new DataTable();
			PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo prop in props)
			{
				var pt = prop.PropertyType;
				if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					table.Columns.Add(prop.Name, pt.GetGenericArguments()[0]);
				}
				else
				{
					table.Columns.Add(prop.Name, prop.PropertyType);
				}
			}

			object[] values = new object[props.Count()];
			foreach (T item in array)
			{
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = props[i].GetValue(item);
				}
				table.Rows.Add(values);
			}

			return table;
		}
	}
}
