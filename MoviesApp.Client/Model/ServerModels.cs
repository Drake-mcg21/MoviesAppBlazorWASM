using System.Text.Json;

namespace MoviesApp.Client.Model
{
    public enum EventType { All, Added, Updated, Removed }

    public sealed class SignalREvent<T>
    {
        public EventType Type { get; set; }
        public T? Payload { get; set; } // array or object
        public Guid? BatchId { get; init; }
        public int? ChunkIndex { get; init; }
        public int? TotalCount { get; init; }
    }

    public record PositionDto(Guid RobotId, double X, double Y, double Yaw, DateTimeOffset Timestamp);
    public record CoreSystemStatusDto
     (
         DateTime TimeStamp,
         string Version,
         bool DatabaseConnection,
         string SystemUptime,
         int Health
     );
    public record APISystemStatusDto(
     DateTime TimeStamp,
     int ActiveClients,
     string SystemUptime
     );

    public static class TopicNames
    {
        public const string V1RobotPositions = "v1/robot/positions";
        public const string V1CoreStatus = "v1/core/status";
        public const string V1ApiStatus = "v1/api/status";
        public const string V1RobotFuturePositions = "v1/robot/futurepositions";
        public const string V1Locations = "v1/locations";
        public const string V1Alarms = "v1/alarms";
        public const string V1Missions = "v1/missions";
        public const string V1Payloads = "v1/payloads";
        public const string V1Devices = "v1/devices";
        public const string V1Robots = "v1/robot";
    }

    public record MissionDto(
       Guid id,
       string name,
       string status,
       string description,
       Guid? assignedRobot,
       string metadata,
       string missionType,
       string fleetManager,
       bool finalized,
       Guid sourcePlaceId,
       Guid loadPlaceId,
       Guid unloadPlaceId,
       Guid destPlaceId,
       DateTimeOffset createdTime
   );
    public record DeviceValueDto(
         Guid deviceId,
         string value,
         DateTimeOffset TimestampUtc
     );
}
