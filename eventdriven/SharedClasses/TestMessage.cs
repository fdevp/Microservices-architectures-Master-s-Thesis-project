using System;

namespace SharedClasses.Messaging
{
    public class TestEvent
    {
        public string Field1 { get; set; }
        public decimal Field2 { get; set; }
        public DateTime Field3 { get; set; }
        public ComplexType[] Field4 { get; set; }

    }

    public class ComplexType
    {
        public string Field5 { get; set; }
        public string[] Field6 { get; set; }
    }
}