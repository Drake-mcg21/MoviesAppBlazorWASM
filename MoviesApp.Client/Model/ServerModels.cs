using System.Text.Json;

namespace MoviesApp.Client.Model
{
    public enum EventType { All, Added, Updated, Removed }

    public sealed class SignalREvent
    {
        public EventType Type { get; set; }
        public JsonElement Payload { get; set; } // array or object
    }

    public record PositionDto(Guid RobotId, double X, double Y, double Yaw, DateTime Timestamp);
}
