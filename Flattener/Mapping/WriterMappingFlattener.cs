using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Flattener.Mapping
{
    public class FlattenerMappingFlattener
    {
        private List<FlattenerMapping> mappings;

        public FlattenerMappingFlattener(List<FlattenerMapping> mappings)
        {
            this.mappings = mappings.ToList();
        }

        public IEnumerable<object[]> FlattenToObjectArrays(IDictionary<string, object> input)
        {
            var flattenedinput = Flatten(input);
            var rows = flattenedinput.FlattenedRows.Select(r => r.FlattenedValues.Select(t => t.Value).ToArray());
            return rows;
        }

        public IEnumerable<IDictionary<int, object>> FlattenToDictionaries(IDictionary<string, object> input)
        {
            var flattenedinput = Flatten(input);
            foreach (var flatrow in flattenedinput.FlattenedRows)
                yield return flatrow.FlattenedValues.ToDictionary(kvp => kvp.Mapping.Destination, k => k.Value);
        }

        public FlattenedInput Flatten(IDictionary<string, object> input)
        {
            var flattenermappings = new List<FlattenerMapping>();
            int dstmappingindex = 0;
            foreach (var mapping in mappings)
            {
                flattenermappings.Add(new FlattenerMapping(mapping.Source, dstmappingindex, mapping.Static));
                dstmappingindex++;
            }

            var flattener = new FlattenerFacade();
            var lines = flattener.Flatten(input, flattenermappings);
            var flattenedinput = new FlattenedInput();
            foreach (var line in lines)
            {
                var inputrow = new FlattenedInputRow();
                for (int i = 0; i < line.Length; i++)
                {
                    var mapping = mappings[i];
                    var inputfield = new FlattenedInputField(mapping, line[i]);
                    inputrow.FlattenedValues.Add(inputfield);
                }
                flattenedinput.FlattenedRows.Add(inputrow);
            }

            //var flattenedrows = lines.Select(l => new FlattenedInputRow() { FlattenedValues = lines.Select(new FlattenedInputField())
            flattenedinput = RemoveDuplicateFields(flattenedinput);
            return flattenedinput;
        }

        private FlattenedInput RemoveDuplicateFields(FlattenedInput input)
        {
            int row = 0;
            var nonduplicateinput = new FlattenedInput();
            foreach (var flattenedrow in input.FlattenedRows)
            {
                var nonduplicaterow = new List<FlattenedInputField>();
                var fieldsgroupedbydestination = flattenedrow.FlattenedValues.GroupBy(r => r.Mapping.Destination).ToList();
                foreach (var fieldgrouped in fieldsgroupedbydestination)
                {
                    var fieldsorderedbydots = fieldgrouped.OrderByDescending(f => f.Mapping.Source.Count(c => c == '.'));
                    foreach (var flattenedvalue in fieldsorderedbydots)
                    {
                        if (fieldsorderedbydots.Last() == flattenedvalue ||
                            (flattenedvalue.Value != null && !string.IsNullOrWhiteSpace(flattenedvalue.Value.ToString())))
                        {
                            nonduplicaterow.Add(flattenedvalue);
                            break;
                        }
                    }
                }
                nonduplicateinput.FlattenedRows.Add(new FlattenedInputRow() { FlattenedValues = nonduplicaterow });
                row++;
            }
            return nonduplicateinput;
        }
    }
}