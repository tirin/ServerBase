using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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

        public long Quantity { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public bool Deleted { get; set; }
    }
}
