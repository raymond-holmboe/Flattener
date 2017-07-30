using System;
using System.Collections.Generic;
using System.Text;

namespace Flattener
{
    public static class Flatten
    {
        public static List<object[]> Flat(List<IDictionary<string, object>> objs, List<FlattenerMapping> mappings)
        {
            var flattener = new Flattener();
            var flattenedrows = flattener.Flatten(objs, mappings);
            return flattenedrows;
        }

        public static List<object[]> Flat(IDictionary<string, object> obj, List<FlattenerMapping> mappings)
        {
            var flattener = new Flattener();
            var flattenedrows = flattener.Flatten(obj, mappings);
            return flattenedrows;
        }

        public static List<object[]> Flat(object obj, List<FlattenerMapping> mappings)
        {
            var flattener = new Flattener();
            var flattenedrows = flattener.Flatten(obj, mappings);
            return flattenedrows;
        }
    }
}
