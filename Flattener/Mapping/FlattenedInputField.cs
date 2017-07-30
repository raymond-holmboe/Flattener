
namespace Flattener.Mapping
{
    public class FlattenedInputField
    {
        public FlattenedInputField(FlattenerMapping mapping, object value)
        {
            Mapping = mapping;
            Value = value;
        }
        public object Value { get; set; }
        public FlattenerMapping Mapping { get; set; }
    }
}
