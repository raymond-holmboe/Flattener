using System.Collections;
using System.Collections.Generic;

namespace Flattener
{
    internal class ObjectToDictionaryConverter
    {
        public IDictionary<string, object> Convert(object source)
        {
            var dictionary = new Dictionary<string, object>();
            Convert(dictionary, source);
            return dictionary;
        }

        private void Convert(IDictionary<string, object> dictionary, object source)
        {
            var properties = source.GetType().GetProperties();
            foreach (var p in properties)
            {
                string key = p.Name;
                object value = p.GetValue(source, null);
                if (value == null)
                {
                    dictionary[key] = null;
                    continue;
                }
                bool islist = p.IsNonStringEnumerable();
                if (islist)
                {   
                    // list of objects
                    var dictlist = new List<IDictionary<string, object>>();
                    foreach (object o in (IEnumerable)value)
                    {
                        var nesteddict = new Dictionary<string, object>();
                        Convert(nesteddict, o);
                        dictlist.Add(nesteddict);
                    }
                    dictionary[key] = dictlist;
                }
                else if (p.PropertyType.IsValueType || value is string)
                {   
                    // normal property
                    dictionary[key] = value;
                }
                else
                {   
                    // object
                    var nesteddict = new Dictionary<string, object>();
                    Convert(nesteddict, value);
                    dictionary[key] = nesteddict;
                }
            }
        }
    }
}
