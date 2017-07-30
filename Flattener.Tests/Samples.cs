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

        public List<object[]> Sample2()
        {
            var order = new Dictionary<string, object>();
            var orderlines = new List<Dictionary<string, object>>();
            orderlines.Add(new Dictionary<string, object> { { "qty", 5 }, { "product", "bar" } });
            orderlines.Add(new Dictionary<string, object> { { "qty", 10 }, { "product", "foobar" } });
            order["customer"] = "foo";
            order["lines"] = orderlines;
            
            var rows = new Flattener().Flatten(order, "customer", "lines.qty", "lines.product");
            return rows;
        }
    }
}
