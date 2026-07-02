using Desktop_Creatures.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace Desktop_Creatures.World
{
    public class PointOfInterestManager
    {

        public List<PointOfInterest> Points { get; } = new();

        public void Add(PointOfInterest poi)
        {
            Points.Add(poi);
        }

        public IEnumerable<PointOfInterest> GetByType(PointOfInterestType type)
        {
            return Points.Where(p => p.Type == type);
        }

        public PointOfInterest? FindNearest(
            Point position,
            PointOfInterestType type)
        {
            //Logger.LogDebug(
            //    $"Searching for nearest {type}. POIs: {Points.Count}");

            var result = Points
                .Where(p => p.Type == type && p.IsAvailable)
                .OrderBy(p => Distance(position, p.Position))
                .FirstOrDefault();

            //Logger.LogDebug(
            //    result is null
            //        ? $"No {type} POI found."
            //        : $"Nearest {type}: {result.Name}");

            return result;
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
