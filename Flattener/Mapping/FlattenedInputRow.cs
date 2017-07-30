using System.Collections.Generic;

namespace Flattener.Mapping
{
    public class FlattenedInputRow
    {
        public List<FlattenedInputField> FlattenedValues { get; set; } = new List<FlattenedInputField>();
    }
}
