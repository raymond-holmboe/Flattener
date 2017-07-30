using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Flattener.Tests")] // placed in an arbitratry .cs file

namespace Flattener.Tokenization
{
    /// <summary>
    /// Collecting metadata and values for a flattener. 
    /// Traverses the object for new values, a flattener needs to fill in the values for the levels using the previous rows.
    /// </summary>
    internal class Tokenizer
    {
        private int currentrow;
        private int currentlevel;
        private Dictionary<int, string> currentparentnames = new Dictionary<int, string>(); // <levelnunmber, propertyname of parent object>
        private List<Token> currentrowtokens = new List<Token>();
        private List<DottedProperty> properties = new List<DottedProperty>();

        public IEnumerable<Token> Tokenize(object obj)
        {
            var converter = new ObjectToDictionaryConverter();
            var dict = converter.Convert(obj);
            return Tokenize(dict);
        }

        public IEnumerable<Token> Tokenize(List<IDictionary<string, object>> exps)
        {
            PrepareTokenization();
            foreach (var exp in exps)
            {
                foreach (var tokens in TokenizeRecursive(exp))
                    yield return tokens;
                currentrow++;
            }
        }

        public IEnumerable<Token> Tokenize(IDictionary<string, object> exp)
        {
            PrepareTokenization();
            var tokens = TokenizeRecursive(exp).ToList();
            return tokens;
        }

        private void PrepareTokenization()
        {
            currentrow = 1;
            currentlevel = 0;
        }

        private IEnumerable<Token> TokenizeRecursive(IDictionary<string, object> input, string sourceprop = null)
        {
            currentlevel++;
            currentparentnames[currentlevel] = sourceprop;
            var dottedinput = input.Select(kvp => new KeyValuePair<DottedProperty, object>(new DottedProperty(GetCurrentDottedProperty(kvp.Key)), kvp.Value));
            var sortedinput = dottedinput.OrderBy(d => d.Value is IList).ThenBy(d => d.Value is IDictionary<string, object>).ToList();
            for (int i = 0; i < sortedinput.Count; i++)
            {
                var kvp = sortedinput[i];
                if (kvp.Value is IList)
                {
                    bool shouldtokenize = ShouldTokenizeObject(kvp.Key.DottedPropertyName);
                    if (shouldtokenize)
                    {
                        var tokens = TokenizeList(kvp.Value, kvp.Key.PropertyName);
                        foreach (var token in tokens)
                            yield return token;
                    }
                }
                else if (kvp.Value is IDictionary<string, object>)
                {
                    bool shouldtokenize = ShouldTokenizeObject(kvp.Key.DottedPropertyName);
                    if (shouldtokenize)
                    {
                        var tokens = TokenizeRecursive(kvp.Value as IDictionary<string, object>, kvp.Key.PropertyName);
                        foreach (var nesteddict in tokens)
                            yield return nesteddict;
                    }
                }
                else
                {
                    // we have reached a normal key/value pair, we will always arrive here in the end
                    if (ShouldCreateToken(kvp.Key))
                    {
                        var token = CreateToken(kvp.Key, kvp.Value);
                        yield return token;
                    }
                }
            }
            currentlevel--;
        }

        private string GetCurrentDottedProperty(string propname)
        {
            string parent = string.Join(".", currentparentnames.Values.Take(currentlevel).ToArray()).TrimStart('.');
            if (string.IsNullOrWhiteSpace(parent))
                return propname;
            string dottedpropname = string.Join(".", parent, propname);
            return dottedpropname;
        }

        private bool ShouldTokenizeObject(string dottedproperty)
        {
            return !properties.Any() || properties.Any(p => p.DottedPropertyName.StartsWith(dottedproperty, StringComparison.OrdinalIgnoreCase)); 
        }

        private bool ShouldCreateToken(DottedProperty prop)
        {
            return !properties.Any() || properties.Any(p => p.DottedPropertyName.Equals(prop.DottedPropertyName, StringComparison.OrdinalIgnoreCase));
        }

        private IEnumerable<Token> TokenizeList(object value, string sourceprop)
        {
            var dictlist = value as IEnumerable<IDictionary<string, object>>;
            // foreach (var nestedexp in ExpandoHelper.ToExpandoList(value))
            foreach (var nestedexp in dictlist)
            {
                var tokens = TokenizeRecursive(nestedexp, sourceprop);
                foreach (var nesteddict in tokens)
                    yield return nesteddict;
                bool shouldadvancerow = currentrowtokens.Any();

                if (shouldadvancerow)
                {
                    currentrowtokens.Clear();
                    currentrow++; // new row on dead ends
                }
            }
        }

        private Token CreateToken(DottedProperty dottedproperty, object value)
        {
            var token = new Token(dottedproperty.DottedPropertyName, value, currentrow, currentlevel);
            currentrowtokens.Add(token);
            return token;
        }

        public void SetDottedProperties(params string[] properties)
        {
            this.properties = properties.Select(p => new DottedProperty(p)).ToList();
        }

    }
}
