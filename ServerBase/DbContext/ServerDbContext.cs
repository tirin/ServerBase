using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServerBase.Entity;

namespace ServerBase
{
    public class ServerDbContext : DbContext
    {
        public ServerDbContext()
        {
        }

        public ServerDbContext(DbContextOptions<ServerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Deleted)
                    .HasDatabaseName("Deleted");
            });

            modelBuilder.Entity<Point>(entity =>
            {
                entity.HasIndex(e => new { e.Deleted, e.UserId, e.Type })
                    .HasDatabaseName("Deleted.UserId.Type");

                entity.Property(e => e.Type)
                    .HasEnumConversion();
            });
        }

        public virtual DbSet<Point> Point { get; set; }
        public virtual DbSet<User> User { get; set; }
    }

    internal static class PropertyBuilderExtends
    {
        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions();

        static PropertyBuilderExtends()
        {
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
            _serializerOptions.IgnoreReadOnlyProperties = true;
        }

        public static PropertyBuilder<TEnum> HasEnumConversion<TEnum>(this PropertyBuilder<TEnum> builder) where TEnum : struct, Enum
        {
            var maxLength = builder.Metadata.GetMaxLength() ?? 16;

            return builder.HasConversion(new EnumToStringConverter<TEnum>())
                .HasColumnType($"char({maxLength})");
        }

        public static PropertyBuilder<TProperty[]> HasJsonArrayConversion<TProperty>(this PropertyBuilder<TProperty[]> builder)
        {
            return builder.HasJsonConversion();
        }

        public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder)
        {
            return builder.HasJsonTextConversion();
        }

        public static PropertyBuilder<TProperty> HasJsonTextConversion<TProperty>(this PropertyBuilder<TProperty> builder)
        {
            builder.Metadata.SetValueComparer(new ValueComparer<TProperty>(
                    (c1, c2) => c1.Equals(c2),
                    c => c.GetHashCode()));

            return builder.HasConversion(
                value => JsonSerializer.Serialize(value, _serializerOptions),
                value => string.IsNullOrEmpty(value) ? default : JsonSerializer.Deserialize<TProperty>(value, _serializerOptions))
                .HasColumnType("TEXT");
        }
    }
}