using Flattener.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flattener
{
    internal class InternalFlattener
    {
        public IEnumerable<object[]> Flatten(IEnumerable<Token> rows, List<FlattenerMapping> mappings)
        {
            var rowsbyrownumber = rows.GroupBy(r => r.Row).OrderBy(r => r.Key);
            Token[] lastrow = null;
            foreach (var tokens in rowsbyrownumber)
            {
                Token[] rowtokens = new Token[mappings.Count];
                foreach (var mapping in mappings)
                {
                    int dstindex = mapping.Destination;
                    var nonstaticonlytoken = tokens.FirstOrDefault(t => !string.IsNullOrWhiteSpace(mapping.Source) && t.Dottedname.Equals(mapping.Source, StringComparison.OrdinalIgnoreCase));
                    if (nonstaticonlytoken != null)
                    {
                        rowtokens[dstindex] = nonstaticonlytoken;
                        if (rowtokens[dstindex].Value == null && !string.IsNullOrWhiteSpace(mapping.Static))
                            rowtokens[dstindex].Value = mapping.Static;
                    }
                    else
                    {
                        var staticonlytoken = tokens.FirstOrDefault(t => !string.IsNullOrWhiteSpace(mapping.Source) && t.Dottedname.Equals(mapping.Source, StringComparison.OrdinalIgnoreCase));
                        if (string.IsNullOrWhiteSpace(mapping.Source) && !string.IsNullOrWhiteSpace(mapping.Static))
                            rowtokens[dstindex] = new Token("", mapping.Static, tokens.Key, 0);
                    }
                }
                if (lastrow != null)
                {
                    var nonnulltokens = rowtokens.Where(r => r != null && r.Level > 0);
                    int minlevelcurrentrow = nonnulltokens.Any() ? nonnulltokens.Min(r => r.Level) : 1;
                    for (int i = 0; i < rowtokens.Length; i++)
                        if (rowtokens[i] == null && lastrow[i] != null && lastrow[i].Level <= minlevelcurrentrow) 
                            rowtokens[i] = lastrow[i];
                }
                yield return rowtokens.Select(r => r?.Value).ToArray();
                lastrow = rowtokens;
            }
        }
    }
}
