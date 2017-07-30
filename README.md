# Flattener
C# .NET Core 2 library, it flattens any nested object or dictionary into object array

Example usage:

var order = new
{
    customer = "foo",
    lines = new[] {
        new { qty = 5, product = "bar", unit = 0 },
        new { qty = 10, product = "foobar", unit = 0 }
    }
};
var rows = new Flattener().Flatten(order, "customer", "lines.qty", "lines.product");

'rows' is a List<object[]> that contains [ "foo", 5, "bar" ] and [ "foo", 10, "foobar" ]


Same result as previous example, but this time the input is nested dictionaries instead of anonymous object:

var order = new Dictionary<string, object>();
var orderlines = new List<Dictionary<string, object>>();
orderlines.Add(new Dictionary<string, object> { { "qty", 5 }, { "product", "bar" } });
orderlines.Add(new Dictionary<string, object> { { "qty", 10 }, { "product", "foobar" } });
order["customer"] = "foo";
order["lines"] = orderlines;

var rows = new Flattener().Flatten(order, "customer", "lines.qty", "lines.product");

'rows' is a List<object[]> that contains [ "foo", 5, "bar" ] and [ "foo", 10, "foobar" ]