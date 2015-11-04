using System;
using System.Collections.Generic;
using System.Reflection;


namespace Scheduler.Service
{
    public partial class Program
    {
        static void writeHeader<T>(T data)
        {
            if (data == null)
            {
                Console.WriteLine("Unable to create Header from Null Object");
                return;
            }
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                Console.Write("{0, 8} ", prop.Name);
            }
            Console.WriteLine();
        }

        static void writeCollectionData<T>(IEnumerable<T> data)
        {
            if (data == null)
            {
                Console.WriteLine("Unable to write data from Null Object");
                return;
            }
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var i = 0;
            foreach (var t in data)
            {
                ++i;
                Console.Write("{0}: ", i);
                writeData(t, props);
                Console.WriteLine();
            }
        }

        static void writeData<T>(T data, IEnumerable<PropertyInfo> propertyInfo = null)
        {
            if (data == null)
            {
                Console.WriteLine("Unable to write data from Null Object");
                return;
            }
            var props = (propertyInfo != null) ? propertyInfo : typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                Console.Write("{0, 8} ", prop.GetValue(data));
            }
        }
    }
}
