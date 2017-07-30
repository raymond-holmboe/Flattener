using System.Linq;

namespace Flattener.Tokenization
{
    internal class Token
    {
        public Token(string dottedname, object value, int row, int level)
        {
            Dottedname = dottedname;
            Name = Dottedname.Split('.').Last();
            Value = value;
            Row = row;
            Level = level;
        }
        public string Dottedname { get; set; } // ex lines.unit.unitname
        public string Name { get; set; } // ex unitname
        public object Value { get; set; }
        public int Row { get; set; } // 1-indexed
        public int Level { get; set; } // 1-indexed

        public override string ToString()
        {
            return $"Row {Row} : {Name}, {Value}. Level {Level}. Dottedname {Dottedname}";
        }
    }
}
