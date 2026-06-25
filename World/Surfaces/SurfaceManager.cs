

namespace Desktop_Creatures.World.Surfaces

{
    public class SurfaceManager
    {
        private readonly List<Surface> _surfaces = new();

        public IReadOnlyList<Surface> Surfaces => _surfaces;

        public void Refresh(Rectangle workingArea)
        {
            _surfaces.Clear();

            // fallback ground
            _surfaces.Add(new Surface(
                new Rectangle(
                    workingArea.Left,
                    workingArea.Bottom - 1,
                    workingArea.Width,
                    1)));
        }

        public Surface? FindSurfaceBelow(
            double x,
            double y,
            int creatureWidth,
            int creatureHeight)
        {
            double feetX = x + creatureWidth / 2.0;
            double feetY = y + creatureHeight;

            return _surfaces
                .Where(s =>
                    feetX >= s.Left &&
                    feetX <= s.Right &&
                    s.Top >= feetY)
                .OrderBy(s => s.Top)
                .FirstOrDefault();
        }

        // find all visible windows

        //convert to surfaces

        //give list to creatures
    }
}
