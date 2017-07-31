# Flattener
## C# .NET Core 2 library, it flattens any nested object or dictionary into object arrays

#### Sample 1: Convert from ```object``` to ```List<object[]>```

```c#
var order = new
{
   customer = "foo",
   lines = new[] {
      new { qty = 5, product = "bar", unit = 0 },
      new { qty = 10, product = "foobar", unit = 0 }
   }
};
var rows = new Flattener().Flatten(order, "customer", "lines.qty", "lines.product");
```

'rows' is a ```List<object[]>``` that contains ```[ "foo", 5, "bar" ]``` and ```[ "foo", 10, "foobar" ]```

#### Sample 2: Convert from nested ```IDictionary<string, object>``` to ```List<object[]>```

```c#
var order = new Dictionary<string, object>();
var orderlines = new List<Dictionary<string, object>>();
orderlines.Add(new Dictionary<string, object> { { "qty", 5 }, { "product", "bar" } });
orderlines.Add(new Dictionary<string, object> { { "qty", 10 }, { "product", "foobar" } });
order["customer"] = "foo";
order["lines"] = orderlines;

var rows = new Flattener().Flatten(order, "customer", "lines.qty", "lines.product");
```

'rows' is a ```List<object[]>``` that contains ```[ "foo", 5, "bar" ]``` and ```[ "foo", 10, "foobar" ]```
