using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Shouldly;
using Flattener.Tokenization;

namespace Flattener.Tests
{
    public class TokenizerTests
    {

        [Fact]
        public void TokenizeNestedLevel3Single()
        {
            var order = new SimpleOrder()
            {
                Customerno = 0,
                Lines = new List<SimpleOrderline>() {
                    new SimpleOrderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now }
                        }
                    }
                }
            };
            var tokenizer = new Tokenizer();
            var rows = tokenizer.Tokenize(order).GroupBy(r => r.Row).OrderBy(r => r.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());
            var firstrow = rows[1];
            firstrow.ShouldContain(r => r.Name == "Customerno" && (int)r.Value == order.Customerno && r.Level == 1);
            firstrow.ShouldContain(r => r.Name == "Productno" && (string)r.Value == order.Lines[0].Productno && r.Level == 2);
            firstrow.ShouldContain(r => r.Name == "Quantity" && (double)r.Value == order.Lines[0].Quantity && r.Level == 2);
            firstrow.ShouldContain(r => r.Name == "Unitname" && (string)r.Value == order.Lines[0].Unit.Unitname && r.Level == 3);
            firstrow.ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[0].Serialnos[0].CreatedAt && r.Level == 3);
            firstrow.ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[0].Serialnos[0].Expired && r.Level == 3);
            firstrow.ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[0].Serialnos[0].Serialno && r.Level == 3);
        }

        [Fact]
        public void TokenizeNestedLevel3Multiple()
        {
            var order = new SimpleOrder()
            {
                Customerno = 0,
                Lines = new List<SimpleOrderline>() {
                    new SimpleOrderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser3", CreatedAt = DateTime.Now },
                        }
                    }
                }
            };
            var tokenizer = new Tokenizer();
            var rows = tokenizer.Tokenize(order).GroupBy(r => r.Row).OrderBy(r => r.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

            rows[1].ShouldContain(r => r.Name == "Customerno" && (int)r.Value == order.Customerno && r.Level == 1);
            rows[1].ShouldContain(r => r.Name == "Productno" && (string)r.Value == order.Lines[0].Productno && r.Level == 2);
            rows[1].ShouldContain(r => r.Name == "Quantity" && (double)r.Value == order.Lines[0].Quantity && r.Level == 2);
            rows[1].ShouldContain(r => r.Name == "Unitname" && (string)r.Value == order.Lines[0].Unit.Unitname && r.Level == 3);
            rows[1].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[0].Serialnos[0].CreatedAt && r.Level == 3);
            rows[1].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[0].Serialnos[0].Expired && r.Level == 3);
            rows[1].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[0].Serialnos[0].Serialno && r.Level == 3);

            rows[2].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[0].Serialnos[1].CreatedAt && r.Level == 3);
            rows[2].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[0].Serialnos[1].Expired && r.Level == 3);
            rows[2].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[0].Serialnos[1].Serialno && r.Level == 3);

            rows[3].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[0].Serialnos[2].CreatedAt && r.Level == 3);
            rows[3].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[0].Serialnos[2].Expired && r.Level == 3);
            rows[3].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[0].Serialnos[2].Serialno && r.Level == 3);
        }

        [Fact]
        public void NestedLineLast()
        {
            var order = new SimpleOrder()
            {
                Customerno = 0,
                Lines = new List<SimpleOrderline>() {
                    new SimpleOrderline() { Quantity = 1, Productno = "1001", Unit = new Unit() { Unitname = "stk" } },
                    new SimpleOrderline() { Quantity = 2, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now },
                        }
                    }
                }
            };
            var tokenizer = new Tokenizer();
            var rows = tokenizer.Tokenize(order).GroupBy(r => r.Row).OrderBy(r => r.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

            rows[1].ShouldContain(r => r.Name == "Customerno" && (int)r.Value == order.Customerno && r.Level == 1);
            rows[1].ShouldContain(r => r.Name == "Productno" && (string)r.Value == order.Lines[0].Productno && r.Level == 2);
            rows[1].ShouldContain(r => r.Name == "Quantity" && (double)r.Value == order.Lines[0].Quantity && r.Level == 2);
            rows[1].ShouldContain(r => r.Name == "Unitname" && (string)r.Value == order.Lines[0].Unit.Unitname && r.Level == 3);

            rows[2].ShouldContain(r => r.Name == "Productno" && (string)r.Value == order.Lines[1].Productno && r.Level == 2);
            rows[2].ShouldContain(r => r.Name == "Quantity" && (double)r.Value == order.Lines[1].Quantity && r.Level == 2);
            rows[2].ShouldContain(r => r.Name == "Unitname" && (string)r.Value == order.Lines[1].Unit.Unitname && r.Level == 3);
            rows[2].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[1].Serialnos[0].CreatedAt && r.Level == 3);
            rows[2].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[1].Serialnos[0].Expired && r.Level == 3);
            rows[2].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[1].Serialnos[0].Serialno && r.Level == 3);

            rows[3].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[1].Serialnos[1].CreatedAt && r.Level == 3);
            rows[3].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[1].Serialnos[1].Expired && r.Level == 3);
            rows[3].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[1].Serialnos[1].Serialno && r.Level == 3);
        }

        [Fact]
        public void NestedLineFirst()
        {
            var order = new SimpleOrder()
            {
                Customerno = 0,
                Lines = new List<SimpleOrderline>() {
                    new SimpleOrderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser3", CreatedAt = DateTime.Now } }
                    },
                    new SimpleOrderline() { Quantity = 3, Productno = "1001", Unit = new Unit() { Unitname = "stk" } },
                }
            };
            var tokenizer = new Tokenizer();
            var rows = tokenizer.Tokenize(order).GroupBy(r => r.Row).OrderBy(r => r.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

            rows[1].ShouldContain(r => r.Name == "Customerno" && (int)r.Value == order.Customerno && r.Level == 1);
            rows[1].ShouldContain(r => r.Name == "Productno" && (string)r.Value == order.Lines[0].Productno && r.Level == 2);
            rows[1].ShouldContain(r => r.Name == "Quantity" && (double)r.Value == order.Lines[0].Quantity && r.Level == 2);
            rows[1].ShouldContain(r => r.Name == "Unitname" && (string)r.Value == order.Lines[0].Unit.Unitname && r.Level == 3);
            rows[1].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[0].Serialnos[0].CreatedAt && r.Level == 3);
            rows[1].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[0].Serialnos[0].Expired && r.Level == 3);
            rows[1].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[0].Serialnos[0].Serialno && r.Level == 3);

            rows[2].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[0].Serialnos[1].CreatedAt && r.Level == 3);
            rows[2].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[0].Serialnos[1].Expired && r.Level == 3);
            rows[2].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[0].Serialnos[1].Serialno && r.Level == 3);

            rows[3].ShouldContain(r => r.Name == "CreatedAt" && (DateTime)r.Value == order.Lines[0].Serialnos[2].CreatedAt && r.Level == 3);
            rows[3].ShouldContain(r => r.Name == "Expired" && (bool)r.Value == order.Lines[0].Serialnos[2].Expired && r.Level == 3);
            rows[3].ShouldContain(r => r.Name == "Serialno" && (string)r.Value == order.Lines[0].Serialnos[2].Serialno && r.Level == 3);

            rows[4].ShouldContain(r => r.Name == "Productno" && (string)r.Value == order.Lines[1].Productno && r.Level == 2);
            rows[4].ShouldContain(r => r.Name == "Quantity" && (double)r.Value == order.Lines[1].Quantity && r.Level == 2);
            rows[4].ShouldContain(r => r.Name == "Unitname" && (string)r.Value == order.Lines[1].Unit.Unitname && r.Level == 3);
        }

        [Fact]
        public void TokenizeWithProperties()
        {
            var order = new SimpleOrder()
            {
                Customerno = 10000,
                Lines = new List<SimpleOrderline>() {
                    new SimpleOrderline() { Quantity = 99, Unit = new Unit() },
                    new SimpleOrderline() { Quantity = 34, Serialnos = null },
                    new SimpleOrderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now } }
                    },
                    new SimpleOrderline() { Quantity = 3, Productno = "1001", Unit = new Unit() { Unitname = "12pack" } },
                    new SimpleOrderline() { Quantity = 100, Productno = "1000000000", Unit = new Unit() { Unitname = "box" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser3", CreatedAt = DateTime.Now } }
                    },
                    new SimpleOrderline() { Quantity = 5 }
                },
                Orderno = "123"
            };
            var tokenizer = new Tokenizer();
            tokenizer.SetDottedProperties("customerno", "lines.quantity", "lines.serialnos.serialno");
            var rawrows = tokenizer.Tokenize(order);
            var rows = rawrows.GroupBy(r => r.Row).OrderBy(r => r.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.ToDictionary(k => k.Dottedname, k => k.Value, StringComparer.OrdinalIgnoreCase));
            rows[1].Count.ShouldBe(2);
            rows[1]["customerno"].ShouldBe(10000);
            rows[1]["lines.quantity"].ShouldBe(99);

            rows[2].Count.ShouldBe(1);
            rows[2]["lines.quantity"].ShouldBe(34);

            rows[3].Count.ShouldBe(2);
            rows[3]["lines.quantity"].ShouldBe(1);
            rows[3]["lines.serialnos.serialno"].ShouldBe("ser1");

            rows[4].Count.ShouldBe(1);
            rows[4]["lines.serialnos.serialno"].ShouldBe("ser2");

            rows[5].Count.ShouldBe(1);
            rows[5]["lines.quantity"].ShouldBe(3);

            rows[6].Count.ShouldBe(2);
            rows[6]["lines.quantity"].ShouldBe(100);
            rows[6]["lines.serialnos.serialno"].ShouldBe("ser3");

            rows[7].Count.ShouldBe(1);
            rows[7]["lines.quantity"].ShouldBe(5);
        }

        [Fact]
        public void RowNumbersCorrectWithFlattenerMappingOnNestedPropertyOnly()
        {
            var order = new SimpleOrder()
            {
                Customerno = 10000,
                Lines = new List<SimpleOrderline>() {
                    new SimpleOrderline() { Quantity = 99, Unit = new Unit() },
                    new SimpleOrderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now } }
                    }
                }
            };
            var tokenizer = new Tokenizer();
            tokenizer.SetDottedProperties("lines.serialnos.serialno");
            var rawrows = tokenizer.Tokenize(order);
            var rows = rawrows.GroupBy(r => r.Row).OrderBy(r => r.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.ToDictionary(k => k.Dottedname, k => k.Value, StringComparer.OrdinalIgnoreCase));
            rows[1].Count.ShouldBe(1);
            rows[1]["lines.serialnos.serialno"].ShouldBe("ser1");
            rows[2].Count.ShouldBe(1);
            rows[2]["lines.serialnos.serialno"].ShouldBe("ser2");

        }

        [Fact]
        public void CombineArraysCorrectly()
        {
            var order = new
            {
                lines = new[] { new { productno = "1001", quantity = 2 } },
                otherlines = new[] { new { productno = "1002", quantity = 4 } }
            };
        }

        [Fact]
        public void DoesNotEnterNestedListsThatHasNoMappingsAndFalselyAdvanceRow()
        {
            var order = new
            {
                orderno = 1033,
                employeeno = 1,
                lines = new[]
                {
                    new
                    {
                        orderline = 1,
                        orderno = 1033,
                        quantity = 3,
                        batches = new List<object>()
                        {
                            new
                            {
                                batchno = 1,
                                quantity = 1
                            }
                        }
                    },
                    new
                    {
                        orderline = 2,
                        orderno = 1033,
                        quantity = 5,
                        batches = new List<object>()
                    }
                }
            };
            var tokenizer = new Tokenizer();
            tokenizer.SetDottedProperties("orderno", "lines.orderline", "lines.quantity", "employeeno");
            var rawrows = tokenizer.Tokenize(order).ToList();
            var rows = rawrows.GroupBy(r => r.Row).OrderBy(r => r.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.ToDictionary(k => k.Dottedname, k => k.Value, StringComparer.OrdinalIgnoreCase));
            rows.Count.ShouldBe(2);
            rows[1].Count.ShouldBe(4);
            rows[1]["orderno"].ShouldBe(1033);
            rows[1]["lines.orderline"].ShouldBe(1);
            rows[1]["lines.quantity"].ShouldBe(3);
            rows[1]["employeeno"].ShouldBe(1);

            rows[2].Count.ShouldBe(2);
            rows[2]["lines.orderline"].ShouldBe(2);
            rows[2]["lines.quantity"].ShouldBe(5);
        }

    }
}
