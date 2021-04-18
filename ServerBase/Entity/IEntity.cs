using System;

namespace ServerBase.Entity
{
    public interface IEntity
    {
        long Id { get; set; }
        DateTime CreateTime { get; set; }
        DateTime? DeleteTime { get; set; }
    }

    public static class IEntityExtensions
    {
        public static string GetWaitingId(this IEntity entity)
        {
            return $"{entity.GetType().Name}:{entity.Id}";
        }
    }
}
