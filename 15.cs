using System;
using System.Text.RegularExpressions;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static partial class Fifteen
    {
        [GeneratedRegex("(-?\\d+)")]
        private static partial Regex MyRegex();

        public static void Run()
        {
            var lines = GetLines();
            var readings = GetReadings(lines).ToArray();
            var reachablePoints = GetReachablePointsOnLine(readings, 2000000);
            Console.WriteLine($"Reachable points: {reachablePoints}");
        }

        static int GetReachablePointsOnLine(IEnumerable<Reading> readings, int line)
        {
            var reachableXCoordinates = new HashSet<long>();
            var sensorBeaconXCoordinates = new HashSet<long>();
            foreach (var reading in readings)
            {
                if (reading.Beacon.Y == line)
                {
                    sensorBeaconXCoordinates.Add(reading.Beacon.X);
                }

                if (reading.Sensor.Y == line)
                {
                    sensorBeaconXCoordinates.Add(reading.Sensor.X);
                }

                var distance = reading.Sensor.ManhattanDistance(reading.Beacon);
                var pointOnLine = new Point(reading.Sensor.X, line);
                var pointBeforeLineDistance = reading.Sensor.ManhattanDistance(pointOnLine);
                var additionalDistance = distance - pointBeforeLineDistance;
                for (var i = 0; i <= additionalDistance; i++)
                {
                    reachableXCoordinates.Add(reading.Sensor.X + i);
                    reachableXCoordinates.Add(reading.Sensor.X - i);
                }
            }

            return reachableXCoordinates.Except(sensorBeaconXCoordinates).Count();
        }

        static IEnumerable<Reading> GetReadings(IEnumerable<string> lines)
        {
            var regex = MyRegex();
            return lines.Select(x =>
            {
                var matches = regex.Matches(x);
                var coordinates = matches.Select(x => long.Parse(x.Value)).ToArray();
                return new Reading(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
            });
        }

        readonly struct Reading
        {
            public Reading(long sensorX, long sensorY, long beaconX, long beaconY)
            {
                Sensor = new Point(sensorX, sensorY);
                Beacon = new Point(beaconX, beaconY);
            }

            public Point Sensor { get; }
            public Point Beacon { get; }
        }

        readonly struct Point
        {
            public Point(long x, long y)
            {
                X = x;
                Y = y;
            }

            public long X { get; }
            public long Y { get; }

            public long ManhattanDistance(Point other)
            {
                return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
            }
        }
    }
}
