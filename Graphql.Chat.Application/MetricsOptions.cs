using App.Metrics;
using App.Metrics.Meter;

namespace GraphQL.Chat.Application
{
    internal static class MetricsOptions
    {
        public static readonly MeterOptions ROOM_OPEN = new MeterOptions
        {
            Name = "Room Open",
            MeasurementUnit = Unit.Events,
            RateUnit = TimeUnit.Seconds
        };
        
        public static readonly MeterOptions ROOM_CLOSE = new MeterOptions
        {
            Name = "Room Close",
            MeasurementUnit = Unit.Events,
            RateUnit = TimeUnit.Seconds
        };
    }
}