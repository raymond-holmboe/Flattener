using System;
using System.Collections.Generic;

namespace Flattener.Tests
{
    public class Serial
    {
        public string Serialno { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Expired { get; set; }
    }

    public class Orderline
    {
        public int Lineno { get; set; }
        public string Productno { get; set; }
        public string GTIN { get; set; }
        public List<Serial> Serialnos { get; set; }
        public Unit Unit { get; set; }
        public double Quantity { get; set; }
        public char TypeCode { get; set; }
    }

    public class Order
    {
        public int Customerno { get; set; }
        public string Customername { get; set; }
        public int Orderno { get; set; }
        public double TotalQuantity { get; set; }
        public string Reference { get; set; }
        public List<Orderline> Lines { get; set; }
        public TimeSpan TimeSinceCreated { get; set; }
        public Freight Freight { get; set; }
    }

    public class Freight
    {
        public double Cost { get; set; }
        public string Carrier { get; set; }
    }

    public class Unit
    {
        public string Unitname { get; set; }
    }

    public class SimpleOrder
    {
        public int Customerno { get; set; }
        public List<SimpleOrderline> Lines { get; set; }
        public string Orderno { get; set; }
    }

    public class SimpleOrderline
    {
        public string Productno { get; set; }
        public List<Serial> Serialnos { get; set; }
        public Unit Unit { get; set; }
        public double Quantity { get; set; }
    }

}
