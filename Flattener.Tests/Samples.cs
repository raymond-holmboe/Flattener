using System;
using System.Collections.Generic;
using System.Text;

namespace Flattener.Tests
{
    public class Samples
    {
        public List<object[]> Sample1()
        {
            var order = new
            {
                customer = "foo",
                lines = new[] {
                    new { qty = 5, product = "bar", unit = 0 },
                    new { qty = 10, product = "foobar", unit = 0 }
                }
            };
            var rows = new Flattener().Flatten(order, "customer", "lines.qty", "lines.product");
            return rows;
        }
    }
}
