using System;

namespace TestTaskCAE.BaseClasses
{
    // This is a bad replacing DTO ;)
    public class Order
    {
        // TODO: If modify - please don't forget about TS order class.
        public Int32 Id { get; set; }
        public DateTime DateCreated { get; set; }
        public Decimal Amount { get; set; }
    }
}
