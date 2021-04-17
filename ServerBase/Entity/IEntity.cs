using System;

namespace ServerBase.Entity
{
    public interface IEntity
    {
        long Id { get; set; }
        DateTime CreateTime { get; set; }
        DateTime? DeleteTime { get; set; }
    }
}
