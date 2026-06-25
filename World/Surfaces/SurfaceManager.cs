

namespace Desktop_Creatures.World.Surfaces

{
    public class SurfaceManager
    {
        private readonly List<Surface> _surfaces = new();

        public IReadOnlyList<Surface> Surfaces => _surfaces;

        public void Refresh()
        {
            _surfaces.Clear();

            foreach (var screen in Screen.AllScreens)
            {
                _surfaces.Add(
                    new Surface(
                        new Rectangle(
                            screen.WorkingArea.Left,
                            screen.WorkingArea.Bottom - 1,
                            screen.WorkingArea.Width,
                            1)));
            }
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
