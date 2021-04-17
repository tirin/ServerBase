using System;
using System.ComponentModel.DataAnnotations;

namespace ServerBase.Entity
{
    public enum PointType
    {
        Gold,
        Silver,
    }

    public class Point 
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        [MaxLength(32)]
        public PointType Type { get; set; }

        [ConcurrencyCheck]
        public long Quantity { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public bool Deleted { get; set; }
    }
}
