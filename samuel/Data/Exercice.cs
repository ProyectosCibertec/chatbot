using Azure;
using Azure.Data.Tables;
using System;

namespace samuel.Data
{
    public class Exercice : ITableEntity
    {
        public string RowKey { get; set; } = default!;

        public string PartitionKey { get; set; } = default!;

        public int Id { get; init; } = default!;

        public string Name { get; init; }

        public string Content { get; init; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
