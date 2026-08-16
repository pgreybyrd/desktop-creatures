using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Forms = System.Windows.Forms;
using Point = System.Windows.Point;

namespace Desktop_Creatures.World.Surfaces;

public class SurfaceManager
{
    private readonly List<Surface> _surfaces = new();
    private int _ticksUntilRefresh = 180;

    public IReadOnlyList<Surface> Surfaces => _surfaces;

    public Surface? MenuSurface { get; private set; }

    public void Refresh()
    {
        _surfaces.Clear();

        AddMonitorGroundSurfaces();
        AddWindowSurfaces();
        AddMenuSurface();
    }

    public void Update()
    {
        _ticksUntilRefresh--;

        if (_ticksUntilRefresh > 0)
            return;

        Refresh();
        _ticksUntilRefresh = 30; // every ~0.5 sec at 60 FPS
    }

    public Point? SnapToSurface(
        Point desiredFeetPosition,
        int creatureWidth,
        int footOffsetY,
        double maxSnapDistance)
    {
        double halfWidth = creatureWidth / 2.0;

        Surface? surface = _surfaces
            .Where(surface =>
                desiredFeetPosition.X >= surface.Left &&
                desiredFeetPosition.X <= surface.Right &&
                Math.Abs(
                    surface.Top -
                    desiredFeetPosition.Y)
                    <= maxSnapDistance)
            .OrderBy(surface =>
                Math.Abs(
                    surface.Top -
                    desiredFeetPosition.Y))
            .FirstOrDefault();

        if (surface is null)
            return null;

        // Keep the entire creature inside the surface.
        double left = surface.Left;
        double right = surface.Right - creatureWidth;

        double creatureX = desiredFeetPosition.X - halfWidth;

        if (right >= left)
            creatureX = Math.Clamp(creatureX, left, right);
        else
            creatureX = left;

        double creatureY = surface.Top - footOffsetY;

        return new Point(creatureX, creatureY);
    }

    public Point? SnapPoiToSurface(
        Point poiPosition,
        double poiWidth,
        double poiHeight,
        double maxSnapDistance)
    {
        double bottomCenterX =
            poiPosition.X + poiWidth / 2.0;

        double bottomY =
            poiPosition.Y + poiHeight;

        Surface? surface =
            _surfaces
                .Where(surface =>
                    bottomCenterX >= surface.Left &&
                    bottomCenterX <= surface.Right &&
                    Math.Abs(
                        surface.Top - bottomY)
                        <= maxSnapDistance)
                .OrderBy(surface =>
                    Math.Abs(
                        surface.Top - bottomY))
                .FirstOrDefault();

        if (surface is null)
            return null;

        double snappedX =
            poiPosition.X;

        double visualOverlap = 5;

        double snappedY =
            surface.Top -
            poiHeight +
            visualOverlap;

        return new Point(
            snappedX,
            snappedY);
    }

    public void AddTemporarySurface(Rectangle bounds)
    {
        _surfaces.Add(new Surface(bounds));
    }

    //private Surface? _menuSurface;

    public void SetMenuSurface(Rectangle bounds)
    {
        MenuSurface = new Surface(bounds);
        MenuSurface.Kind = "Menu Surface";
    }

    private void AddMenuSurface()
    {
        if (MenuSurface is not null)
            _surfaces.Add(MenuSurface);
    }

    public Surface? FindSurfaceBelow(
        double x,
        double y,
        int creatureWidth,
        int footOffsetY)
    { 
        double feetX = x + creatureWidth / 2.0;
        double feetY = y + footOffsetY;

        return _surfaces
            .Where(s =>
            feetX >= s.Left &&
            feetX <= s.Right &&
            s.Top >= feetY)
            .OrderBy(s => s.Top)
            .FirstOrDefault();
    }

    public Surface? FindSurfaceAtFeet(
        double x,
        double y,
        int creatureWidth,
        int creatureHeight,
        double tolerance)
    {
        double feetX = x + creatureWidth / 2.0;
        double feetY = y + creatureHeight;

        return _surfaces
            .Where(s =>
                feetX >= s.Left &&
                feetX <= s.Right &&
                Math.Abs(feetY - s.Top) <= tolerance)
            .OrderBy(s => Math.Abs(feetY - s.Top))
            .FirstOrDefault();
    }

    private void AddMonitorGroundSurfaces()
    {
        var groundRects =
            new List<Rectangle>();

        foreach (var screen in Forms.Screen.AllScreens)
        {
            var workArea =
                ToDipRectangle(screen.WorkingArea);

            var screenArea =
                ToDipRectangle(screen.Bounds);

            groundRects.Add(
                new Rectangle(
                    workArea.Left,
                    workArea.Bottom - 1,
                    workArea.Width,
                    1));

            // Keep safety floors separate.
            if (screenArea.Bottom != workArea.Bottom)
            {
                _surfaces.Add(
                    new Surface(
                        new Rectangle(
                            screenArea.Left,
                            screenArea.Bottom - 1,
                            screenArea.Width,
                            1),
                        "MonitorSafetyGround"));
            }
        }

        foreach (Rectangle merged in
                 MergeHorizontalGroundSurfaces(groundRects))
        {
            _surfaces.Add(
                new Surface(
                    merged,
                    "MonitorGround"));
        }
    }

    private static IEnumerable<Rectangle>
        MergeHorizontalGroundSurfaces(
            IEnumerable<Rectangle> surfaces)
    {
        var remaining =
            surfaces
                .OrderBy(rect => rect.Top)
                .ThenBy(rect => rect.Left)
                .ToList();

        while (remaining.Count > 0)
        {
            Rectangle current =
                remaining[0];

            remaining.RemoveAt(0);

            bool merged;

            do
            {
                merged = false;

                for (int i = 0;
                     i < remaining.Count;
                     i++)
                {
                    Rectangle candidate =
                        remaining[i];

                    // Must be the exact same floor level.
                    if (candidate.Top != current.Top)
                        continue;

                    // Must actually touch or overlap horizontally.
                    const int edgeTolerance = 2;

                    bool touches =
                        candidate.Left <=
                            current.Right + edgeTolerance &&
                        candidate.Right >=
                            current.Left - edgeTolerance;

                    if (Math.Abs(
                        candidate.Top -
                        current.Top) > 2)
                    {
                        continue;
                    }

                    if (!touches)
                        continue;

                    int left =
                        Math.Min(
                            current.Left,
                            candidate.Left);

                    int right =
                        Math.Max(
                            current.Right,
                            candidate.Right);

                    current =
                        new Rectangle(
                            left,
                            current.Top,
                            right - left,
                            1);

                    remaining.RemoveAt(i);

                    merged = true;
                    break;
                }
            }
            while (merged);

            yield return current;
        }
    }

    public Rectangle GetMonitorBoundsUnderCursor()
    {
        var screen =
            Forms.Screen.FromPoint(
                Forms.Cursor.Position);

        return ToDipRectangle(
            screen.Bounds);
    }

    private bool LooksLikeMonitorShell(RECT rect)
    {
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        foreach (var screen in Forms.Screen.AllScreens)
        {
            var b = screen.Bounds;

            bool sameLeft = Math.Abs(rect.Left - b.Left) < 20;
            bool sameTop = Math.Abs(rect.Top - b.Top) < 40;
            bool sameWidth = Math.Abs(width - b.Width) < 40;
            bool almostFullHeight = height > b.Height * 0.8;

            if (sameLeft && sameTop && sameWidth && almostFullHeight)
                return true;
        }

        return false;
    }

    private void AddWindowSurfaces()
    {
        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint processId);

            if (processId == Environment.ProcessId)
                return true; // skip my own app windows

            if (!IsWindowVisible(hWnd))
                return true;

            if (IsWindowCloaked(hWnd))
                return true;

            if (IsIconic(hWnd))
                return true;

            GetWindowRect(hWnd, out RECT rect);

            if (!IsTopEdgeExposed(hWnd, rect))
                return true;

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (LooksLikeMonitorShell(rect))
                return true;

            if (width < 100 || height < 80)
                return true;

            string title = GetWindowTitle(hWnd);

            if (string.IsNullOrWhiteSpace(title))
                return true;

            var pixelRect = new Rectangle(
                rect.Left,
                rect.Top,
                width,
                height);

            var dipRect = ToDipRectangle(pixelRect);

            _surfaces.Add(new Surface(
                new Rectangle(
                    dipRect.Left,
                    dipRect.Top,
                    dipRect.Width,
                    1),
                $"Window: {title}"));

            return true;
        }, IntPtr.Zero);
    }

    private static Rectangle ToDipRectangle(Rectangle pixelRect)
    {
        var source = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);

        if (source?.CompositionTarget is null)
            return pixelRect;

        var transform = source.CompositionTarget.TransformFromDevice;

        var topLeft = transform.Transform(
            new System.Windows.Point(pixelRect.Left, pixelRect.Top));

        var bottomRight = transform.Transform(
            new System.Windows.Point(pixelRect.Right, pixelRect.Bottom));

        return new Rectangle(
            (int)Math.Round(topLeft.X),
            (int)Math.Round(topLeft.Y),
            (int)Math.Round(bottomRight.X - topLeft.X),
            (int)Math.Round(bottomRight.Y - topLeft.Y));
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        int length = GetWindowTextLength(hWnd);
        if (length == 0)
            return "";

        var builder = new StringBuilder(length + 1);
        GetWindowText(hWnd, builder, builder.Capacity);
        return builder.ToString();
    }

    private static bool IsWindowCloaked(IntPtr hWnd)
    {
        const int DWMWA_CLOAKED = 14;

        int cloaked = 0;

        int result = DwmGetWindowAttribute(
            hWnd,
            DWMWA_CLOAKED,
            out cloaked,
            Marshal.SizeOf<int>());

        return result == 0 && cloaked != 0;
    }

    private static bool IsTopEdgeExposed(
    IntPtr hWnd,
    RECT rect)
    {
        int width =
            rect.Right - rect.Left;

        if (width <= 0)
            return false;

        // Sample several places slightly inside the
        // window's top edge.
        double[] samplePositions =
        {
        0.10,
        0.25,
        0.50,
        0.75,
        0.90
    };

        const int sampleYOffset = 8;

        foreach (double position in samplePositions)
        {
            int x =
                rect.Left +
                (int)(width * position);

            int y =
                rect.Top +
                sampleYOffset;

            IntPtr windowAtPoint =
                WindowFromPoint(
                    new POINT
                    {
                        X = x,
                        Y = y
                    });

            if (windowAtPoint == IntPtr.Zero)
                continue;

            // WindowFromPoint can return a child control,
            // so compare its root window with our candidate.
            IntPtr rootWindow =
                GetAncestor(
                    windowAtPoint,
                    GA_ROOT);

            if (rootWindow == hWnd)
                return true;
        }

        return false;
    }
    //Phantom window detection code included!! rawr

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        IntPtr hWnd,
        int dwAttribute,
        out int pvAttribute,
        int cbAttribute);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private const uint GA_ROOT = 2;

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(
        POINT point);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(
        IntPtr hWnd,
        uint gaFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}