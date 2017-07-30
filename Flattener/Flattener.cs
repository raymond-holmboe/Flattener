using Flattener.Tokenization;
using System.Collections.Generic;
using System.Linq;

namespace Flattener
{
    public class Flattener
    {
        public List<object[]> Flatten(object input, params string[] fields)
        {
            var flattener = new Flattener();
            var mappings = new List<FlattenerMapping>();
            for (int i = 0; i < fields.Length; i++)
                mappings.Add(new FlattenerMapping(fields[i], i));
            var flattenedrows = flattener.Flatten(input, mappings);
            return flattenedrows;
        }

        public List<object[]> Flatten(IDictionary<string, object> input, params string[] fields)
        {
            var flattener = new Flattener();
            var mappings = new List<FlattenerMapping>();
            for (int i = 0; i < fields.Length; i++)
                mappings.Add(new FlattenerMapping(fields[i], i));
            var flattenedrows = flattener.Flatten(input, mappings);
            return flattenedrows;
        }

        public List<object[]> Flatten(object input, List<FlattenerMapping> mappings)
        {
            var objectconverter = new ObjectToDictionaryConverter();
            var dict = objectconverter.Convert(input);
            return Flatten(dict, mappings);
        }

        public List<object[]> Flatten(IDictionary<string, object> input, List<FlattenerMapping> mappings)
        {
            var tokens = Tokenize(input, mappings);
            var flattener = new InternalFlattener();
            List<object[]> rows = flattener.Flatten(tokens, mappings).ToList();
            return rows;
        }

        public List<object[]> Flatten(List<IDictionary<string, object>> inputlist, List<FlattenerMapping> mappings)
        {
            var tokens = Tokenize(inputlist, mappings).ToList();
            var flattener = new InternalFlattener();
            List<object[]> rows = flattener.Flatten(tokens, mappings).ToList();
            return rows;
        }

        private IEnumerable<Token> Tokenize(List<IDictionary<string, object>> inputlist, List<FlattenerMapping> mappings)
        {
            var tokenizer = new Tokenizer();
            tokenizer.SetDottedProperties(mappings.Select(m => m.Source).ToArray());
            var tokens = tokenizer.Tokenize(inputlist);
            var statictokens = GenerateStaticTokens(tokens, mappings);
            return tokens.Concat(statictokens);
        }

        private IEnumerable<Token> Tokenize(IDictionary<string, object> input, List<FlattenerMapping> mappings)
        {
            var tokenizer = new Tokenizer();
            tokenizer.SetDottedProperties(mappings.Select(m => m.Source).ToArray());
            var tokens = tokenizer.Tokenize(input);
            var statictokens = GenerateStaticTokens(tokens, mappings);
            return tokens.Concat(statictokens);
        }

        private List<Token> GenerateStaticTokens(IEnumerable<Token> tokens, List<FlattenerMapping> mappings)
        {
            var statictokens = new List<Token>();
            var staticmappings = mappings.Where(m => string.IsNullOrWhiteSpace(m.Source) && !string.IsNullOrWhiteSpace(m.Static));
            if (!staticmappings.Any())
                return statictokens;
            if (!tokens.Any())
                return staticmappings.Select(s => new Token("", s.Static, 0, 0)).ToList();
            var rowtokens = tokens.GroupBy(r => r.Row);
            foreach (var token in rowtokens)
            {
                var mappingtokens = staticmappings.Select(s => new Token("", s.Static, token.Key, 0)).ToList();
                statictokens.AddRange(mappingtokens);
            }
            return statictokens;
        }
    }
}
