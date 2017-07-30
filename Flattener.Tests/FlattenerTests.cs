using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Shouldly;
using Flattener.Tokenization;
using Newtonsoft.Json;

namespace Flattener.Tests
{
    public class FlattenerTests
    {

        [Fact]
        public void ObjectWithArrayIsWritten()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
                Lines = new List<Orderline>() {
                    new Orderline() { Quantity = 1, Productno = "1" },
                    new Orderline() { Quantity = 2, Productno = "2" },
                    new Orderline() { Quantity = 3, Productno = "3" },
                    new Orderline() { Quantity = 4, Productno = "4" }
                }
            };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0 ),
                new FlattenerMapping("Customername", 1 ),
                new FlattenerMapping("Lines.Quantity", 2 ),
                new FlattenerMapping("Lines.Productno", 3),
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows.Count.ShouldBe(4);
            rows.ShouldAllBe(d => d[2].ToString() == d[3] as string);
            rows.ShouldAllBe(d => (string)d[1] == "Bx");
            rows.ShouldAllBe(d => (int)d[0] == 0);
        }

        [Fact]
        public void TestLargeObjectOneRowFromFile()
        {
            string json = File.ReadAllText("TestLargeObjectOneRowInput.txt");
            var jsonobj = JsonConvert.DeserializeObject<Order>(json);
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
                new FlattenerMapping("Lines.Productno", 2),
                new FlattenerMapping("Lines.Quantity", 3),
            };
            var rows = new Flattener().Flatten(jsonobj, mappings);
            rows.Count.ShouldBe(1);

            rows[0].ShouldBe(new object[] {
                jsonobj.Customerno, jsonobj.Customername, jsonobj.Lines[0].Productno, jsonobj.Lines[0].Quantity });
        }

        [Fact]
        public void TestRootOnlyObject()
        {
            var order = new Order() { Customerno = 0, Customername = "TestCustomer" };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
            };
            var rows = new Flattener().Flatten(order, mappings);

            rows.Count.ShouldBe(1);
            rows.ShouldAllBe(d => (string)d[1] == "TestCustomer");
            rows.ShouldAllBe(d => (int)d[0] == 0);
        }


        [Fact]
        public void IgnoresMappingIfFieldMissingInInput()
        {
            var order = new Order()
            {
                Customerno = 10,
                Orderno = 500,
                Lines = new List<Orderline>() {
                    new Orderline() { Quantity = 1, Productno = "8" },
                    new Orderline() { Quantity = 3, Productno = "2" } }
            };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
                new FlattenerMapping("Lines.Productno", 2),
            };
            var rows = new Flattener().Flatten(order, mappings);

            rows.Count.ShouldBe(2);
            rows[0][0].ShouldBe(10);
            rows[0][1].ShouldBeNull();
            rows[0][2].ShouldBe("8");
        }

        [Fact]
        public void ValueOverridesStatic()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
            };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1) { Static = "static" },
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows[0][1].ShouldBe("Bx");
        }

        [Fact]
        public void UseStaticWhenEmptySourceMapping()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
            };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("", 1) { Static = "static" },
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows[0][1].ShouldBe("static");
        }

        [Fact]
        public void UseStaticOnEmptyInput()
        {
            var order = new { };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("", 0) { Static = "static" },
            };
            List<object[]> rows = new Flattener().Flatten(order, mappings);
            var staticrow = rows.ShouldHaveSingleItem();
            var staticvalue = staticrow.ShouldHaveSingleItem();
            staticvalue.ShouldBe("static");
        }

        [Fact]
        public void UseOnlyStatic()
        {
            var order = new { orderno = 1 };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("", 0) { Static = "static1" },
                new FlattenerMapping("", 1) { Static = "static2" },
            };
            List<object[]> rows = new Flattener().Flatten(order, mappings);
            rows.Count().ShouldBe(1);
            rows[0].Count().ShouldBe(2);
            rows[0][0].ShouldBe("static1");
            rows[0][1].ShouldBe("static2");
        }

        [Fact]
        public void TestDuplicateDestinationIndices()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
            };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
                new FlattenerMapping("Customername", 2),
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows.Count.ShouldBe(1);
            // CollectionAssert.AreEqual(rows[0], new object[] { 0, "Bx", "Bx" });
            rows[0].ToList().ShouldBe(new List<object> { 0, "Bx", "Bx" });
        }

        [Fact]
        public void TestObjectWithNestedObjectInArray()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
                Lines = new List<Orderline>() {
                    new Orderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" } },
                    new Orderline() { Quantity = 3, Productno = "1001", Unit = new Unit() { Unitname = "12pack" } }
                }
            };

            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
                new FlattenerMapping("Lines.Productno", 2),
                new FlattenerMapping("Lines.Unit.Unitname", 3),
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows.Count.ShouldBe(2);
            // CollectionAssert.AreEqual(rows[0], new object[] {
            //     order.Customerno, order.Customername, order.Lines[0].Productno, order.Lines[0].Unit.Unitname });
            // CollectionAssert.AreEqual(rows[1], new object[] {
            //    order.Customerno, order.Customername, order.Lines[1].Productno, order.Lines[1].Unit.Unitname });

            rows[0].ToList().ShouldBe(new List<object> {
                order.Customerno, order.Customername, order.Lines[0].Productno, order.Lines[0].Unit.Unitname });
            rows[1].ToList().ShouldBe(new List<object> {
                order.Customerno, order.Customername, order.Lines[1].Productno, order.Lines[1].Unit.Unitname });

        }

        [Fact]
        public void TestArraysFollowedByEmptyNestedObject()
        {
            var order = new Order()
            {
                Customerno = 0,
                Lines = new List<Orderline>() {
                    new Orderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now } }
                    },
                    new Orderline() { Quantity = 3, Productno = "1001", Unit = new Unit() }
                }
            };

            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Lines.Productno", 1),
                new FlattenerMapping("Lines.Serialnos.Serialno", 2),
                new FlattenerMapping("Lines.Unit.Unitname", 3)
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows.Count.ShouldBe(3);
            //CollectionAssert.AreEqual(rows[0], new object[]
            //    { order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[0].Serialno, order.Lines[0].Unit.Unitname });
            //CollectionAssert.AreEqual(rows[1], new object[]
            //    { order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[1].Serialno, order.Lines[0].Unit.Unitname });
            //CollectionAssert.AreEqual(rows[2], new object[]
            //    { order.Customerno, order.Lines[1].Productno, null, order.Lines[1].Unit.Unitname });

            rows[0].ToList().ShouldBe(new List<object> {
                order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[0].Serialno, order.Lines[0].Unit.Unitname });
            rows[1].ToList().ShouldBe(new List<object> {
                order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[1].Serialno, order.Lines[0].Unit.Unitname });
            rows[2].ToList().ShouldBe(new List<object> {
                order.Customerno, order.Lines[1].Productno, null, order.Lines[1].Unit.Unitname });
        }

        [Fact]
        public void TestArraysFollowedByNonExistingNestedObject()
        {
            var order = new Order()
            {
                Customerno = 0,
                Lines = new List<Orderline>() {
                    new Orderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now } }
                    },
                    new Orderline() { Quantity = 3, Productno = "1001" }
                }
            };

            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Lines.Productno", 1),
                new FlattenerMapping("Lines.Serialnos.Serialno", 2),
                new FlattenerMapping("Lines.Unit.Unitname", 3)
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows.Count.ShouldBe(3);
            //CollectionAssert.AreEqual(rows[0], new object[]
            //    { order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[0].Serialno, order.Lines[0].Unit.Unitname });
            //CollectionAssert.AreEqual(rows[1], new object[]
            //    { order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[1].Serialno, order.Lines[0].Unit.Unitname });
            //CollectionAssert.AreEqual(rows[2], new object[]
            //    { order.Customerno, order.Lines[1].Productno, null, null});

            rows[0].ToList().ShouldBe(new List<object> {
                order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[0].Serialno, order.Lines[0].Unit.Unitname });
            rows[1].ToList().ShouldBe(new List<object> {
               order.Customerno, order.Lines[0].Productno, order.Lines[0].Serialnos[1].Serialno, order.Lines[0].Unit.Unitname });
            rows[2].ToList().ShouldBe(new List<object> {
               order.Customerno, order.Lines[1].Productno, null, null });
        }

        [Fact]
        public void TestObjectWithNestedArraysAndNestedObjectInArray()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
                Lines = new List<Orderline>() {
                    new Orderline() { Quantity = 1, Productno = "1000", Unit = new Unit() { Unitname = "pcs" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser1", CreatedAt = DateTime.Now },
                        new Serial() { Serialno = "ser2", CreatedAt = DateTime.Now } }
                    },
                    new Orderline() { Quantity = 3, Productno = "1001", Unit = new Unit() { Unitname = "12pack" } },
                    new Orderline() { Quantity = 100, Productno = "1000000000", Unit = new Unit() { Unitname = "box" }, Serialnos = new List<Serial>() {
                        new Serial() { Serialno = "ser3", CreatedAt = DateTime.Now } }
                    },
                },
                TotalQuantity = 3
            };

            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
                new FlattenerMapping("Lines.Productno", 2),
                new FlattenerMapping("Lines.Unit.Unitname", 3),
                new FlattenerMapping("Lines.Serialnos.Serialno", 4),
                new FlattenerMapping("TotalQuantity", 5),
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows.Count.ShouldBe(4);
            //CollectionAssert.AreEqual(rows[0], new List<object>
            //    { order.Customerno, order.Customername, order.Lines[0].Productno, order.Lines[0].Unit.Unitname, order.Lines[0].Serialnos[0].Serialno, order.TotalQuantity });
            //CollectionAssert.AreEqual(rows[1], new List<object>
            //    { order.Customerno, order.Customername, order.Lines[0].Productno, order.Lines[0].Unit.Unitname, order.Lines[0].Serialnos[1].Serialno, order.TotalQuantity });
            //CollectionAssert.AreEqual(rows[2], new List<object>
            //    { order.Customerno, order.Customername, order.Lines[1].Productno, order.Lines[1].Unit.Unitname, null, order.TotalQuantity });
            //CollectionAssert.AreEqual(rows[3], new List<object>
            //    { order.Customerno, order.Customername, order.Lines[2].Productno, order.Lines[2].Unit.Unitname, order.Lines[2].Serialnos[0].Serialno, order.TotalQuantity });
            rows[0].ToList().ShouldBe(new List<object> {
                order.Customerno, order.Customername, order.Lines[0].Productno, order.Lines[0].Unit.Unitname, order.Lines[0].Serialnos[0].Serialno, order.TotalQuantity });
            rows[1].ToList().ShouldBe(new List<object> {
               order.Customerno, order.Customername, order.Lines[0].Productno, order.Lines[0].Unit.Unitname, order.Lines[0].Serialnos[1].Serialno, order.TotalQuantity });
            rows[2].ToList().ShouldBe(new List<object> {
               order.Customerno, order.Customername, order.Lines[1].Productno, order.Lines[1].Unit.Unitname, null, order.TotalQuantity });
            rows[3].ToList().ShouldBe(new List<object> {
               order.Customerno, order.Customername, order.Lines[2].Productno, order.Lines[2].Unit.Unitname, order.Lines[2].Serialnos[0].Serialno, order.TotalQuantity });
        }

        [Fact]
        public void TestObjectWithEmptyArray()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
                Lines = new List<Orderline>()

            };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
                new FlattenerMapping("Lines.Productno", 2),
            };
            var rows = new Flattener().Flatten(order, mappings);
            rows.Count.ShouldBe(1);
            // CollectionAssert.AreEqual(rows[0], new object[] { order.Customerno, order.Customername, null });
            rows[0].ToList().ShouldBe(new List<object> { order.Customerno, order.Customername, null });
        }


        [Fact]
        public void TestObjectWithMissingSourceProperty()
        {
            var order = new Order()
            {
                Customerno = 0,
                Customername = "Bx",
                Lines = new List<Orderline>()
            };
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Employee", 1),
                new FlattenerMapping("Customername", 2),
                new FlattenerMapping("Lines.Productno",3),
            };
            var rows = new Flattener().Flatten(order, mappings);
            var flattened = rows.ShouldHaveSingleItem();
            //CollectionAssert.AreEqual(flattened, new object[] { order.Customerno, null, order.Customername, null });
            flattened.ShouldBe(new List<object> { order.Customerno, null, order.Customername, null });
        }

        [Fact]
        public void TestSalesOrderObject()
        {
            var json = "{\"OriginalOrder\":null,\"Id\":\"a30941ef-a8a7-468f-9a43-95cf381b1cde\",\"ImportId\":null,\"Orderno\":0,\"Date\":\"\\/Date(1466583116000+0200)\\/\",\"DelDate\":\"\\/Date(-6847808400000+0100)\\/\",\"Ordertype\":\"normal\",\"Orderstatus\":\"new\",\"Customerno\":10000,\"Customername\":\"ABB Installasjon AS\",\"Warehouse\":null,\"DelName\":null,\"DelAddress\":\"\",\"DelPostcode\":\"\",\"DelCity\":\"\",\"Orderlines\":0,\"RemainingPickQuantity\":0.0,\"TotalPrice\":0.0,\"TotalTax\":0.0,\"OurRef\":\"\",\"YourRef\":\"\",\"CustPO\":null,\"Contact\":0,\"ContactName\":null,\"Project\":0,\"Projectname\":null,\"Employee\":1,\"Employeename\":\"Per Pedersen\",\"Locked\":false,\"LockedBy\":null,\"ColorCode\":null,\"Lines\":[{\"Orderno\":0,\"Orderline\":0,\"Productno\":\"1001\",\"Productname\":\"Olyo MS 0703 Carbon Crown Rescue Wood\",\"Location\":\"1339\",\"Warehouse\":\"\",\"DelDate\":\"\\/Date(-62135596800000+0100)\\/\",\"Comment\":null,\"Price\":995.0,\"Quantity\":2.0,\"Discount1\":0.0,\"Discount2\":0.0,\"Discount3\":0.0,\"ReservedQuantity\":0.0,\"PickingQuantity\":1.0,\"PickedQuantity\":0.0,\"Unitname\":null,\"Unitfactor\":0.0,\"Gtin\":\"123456789\",\"Plu\":\"\",\"Project\":0,\"Projectname\":null,\"FreeQuantity\":11.5}],\"New\":true,\"Signature\":null,\"SignatureRotation\":\"Rotate90FlipNone\"}";
            var jsonobj = JsonConvert.DeserializeObject<Order>(json);
            var mappings = new List<FlattenerMapping>()
            {
                new FlattenerMapping("Customerno", 0),
                new FlattenerMapping("Customername", 1),
                new FlattenerMapping("Lines.Quantity", 2),
                new FlattenerMapping("Lines.Productno", 3),
            };
            var rows = new Flattener().Flatten(jsonobj, mappings);
            var flattened = rows.ShouldHaveSingleItem();
        }

        [Fact]
        public void FlattenComplexObject()
        {
            var order = new SimpleOrder()
            {
                Customerno = 0,
                Lines = new List<SimpleOrderline>() {
                    new SimpleOrderline() { Quantity = 50, Productno = "1", Unit = new Unit() },
                    new SimpleOrderline() { Quantity = 100, Unit = null },
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
            var mappings = new List<FlattenerMapping>() { new FlattenerMapping("lines.productno", 0), new FlattenerMapping("lines.serialnos.serialno", 1) };
            var tokenizer = new Tokenizer();
            tokenizer.SetDottedProperties(mappings.Select(m => m.Source).ToArray());
            var tokens = tokenizer.Tokenize(order);
            var flattener = new InternalFlattener();
            var rows = flattener.Flatten(tokens, mappings).ToList();
            rows[0].ShouldBe(new object[] { "1", null });
            rows[1].ShouldBe(new object[] { null, null });
            rows[2].ShouldBe(new object[] { "1000", "ser1" });
            rows[3].ShouldBe(new object[] { "1000", "ser2" });
            rows[4].ShouldBe(new object[] { "1001", null });
            rows[5].ShouldBe(new object[] { "1000000000", "ser3" });
            rows[6].ShouldBe(new object[] { null, null });
        }
    }
}
