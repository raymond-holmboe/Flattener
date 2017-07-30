using System.Linq;

namespace Flattener.Tokenization
{
    internal class DottedProperty
    {
        public string DottedPropertyName { get; set; }
        public string PropertyName { get; set; }

        public DottedProperty(string dottedpropertyname)
        {
            DottedPropertyName = dottedpropertyname;
            string[] nameparts = DottedPropertyName.Split('.');
            PropertyName = nameparts.Last();
        }

        public override string ToString()
        {
            return DottedPropertyName;
        }

    }
}
