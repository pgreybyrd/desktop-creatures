using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Forms = System.Windows.Forms;
using Point = System.Windows.Point;

namespace Desktop_Creatures.World.Surfaces;

public class SurfaceManager
{
    private readonly List<Surface> _surfaces = new();

    private readonly Dictionary<
        string,
        Func<Rectangle?>> _appSurfaces =
            new();

    private int _ticksUntilRefresh = 180;

    public IReadOnlyList<Surface> Surfaces => _surfaces;

    public Surface? MenuSurface { get; private set; }

    public void Refresh()
    {
        List<Surface> previousSurfaces =
            _surfaces.ToList();

        _surfaces.Clear();

        AddMonitorGroundSurfaces(
            previousSurfaces);

        AddTaskbarGround(
            previousSurfaces);

        AddWindowSurfaces();

        AddMenuSurface();
        AddAppSurfaces();
    }

    public void Update()
    {
        _ticksUntilRefresh--;

        if (_ticksUntilRefresh > 0)
            return;

        Refresh();
        _ticksUntilRefresh = 30; // every ~0.5 sec at 60 FPS
    }

    public void RegisterAppSurface(
        string id,
        Func<Rectangle?> boundsProvider)
    {
        _appSurfaces[id] =
            boundsProvider;

        Refresh();
    }

    public void RemoveAppSurface(
        string id)
    {
        _appSurfaces.Remove(id);

        Refresh();
    }

    private void AddAppSurfaces()
    {
        foreach (var pair in
            _appSurfaces)
        {
            Rectangle? bounds =
                pair.Value();

            if (bounds is null)
                continue;

            _surfaces.Add(
                new Surface(
                    bounds.Value,
                    $"AppSurface: {pair.Key}"));
        }
    }

    private void AddTaskbarGround(
        IReadOnlyList<Surface> previousSurfaces)
    {
        if (IsTaskbarAutoHideEnabled())
            return;

        Forms.Screen primaryScreen =
            Forms.Screen.PrimaryScreen!;

        Rectangle screenArea =
            ToDipRectangle(
                primaryScreen.Bounds);

        Rectangle workArea =
            ToDipRectangle(
                primaryScreen.WorkingArea);

        if (workArea.Bottom >=
            screenArea.Bottom)
        {
            return;
        }

        Rectangle bounds =
            new(
                screenArea.Left,
                workArea.Bottom - 1,
                screenArea.Width,
                1);

        _surfaces.Add(
            ReuseOrCreateSurface(
                bounds,
                "TaskbarGround",
                previousSurfaces));
    }

    private Surface ReuseOrCreateSurface(
        Rectangle bounds,
        string kind,
        IReadOnlyList<Surface> previousSurfaces)
    {
        Surface? existing =
            previousSurfaces
                .FirstOrDefault(
                    surface =>
                        surface.Kind == kind &&
                        surface.Bounds == bounds);

        return existing ??
            new Surface(
                bounds,
                kind);
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

    public Surface? FindMonitorGroundBelow(
     double x,
     double y,
     double tolerance)
    {
        return _surfaces
            .Where(surface =>
                surface.Kind == "MonitorGround" &&
                x >= surface.Left &&
                x <= surface.Right &&
                surface.Top >= y - tolerance)
            .OrderBy(surface =>
                surface.Top)
            .FirstOrDefault();
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

    private void AddMonitorGroundSurfaces(
        IReadOnlyList<Surface> previousSurfaces)
    {
        foreach (var screen in
                 Forms.Screen.AllScreens)
        {
            Rectangle screenArea =
                ToDipRectangle(
                    screen.Bounds);

            Rectangle bounds =
                new(
                    screenArea.Left,
                    screenArea.Bottom - 1,
                    screenArea.Width,
                    1);

            _surfaces.Add(
                ReuseOrCreateSurface(
                    bounds,
                    "MonitorGround",
                    previousSurfaces));
        }
    }

    public Rectangle GetWalkableSpan(
        Surface origin,
        double verticalTolerance)
    {
        List<Surface> connected =
            GetConnectedWalkableSurfaces(
                origin,
                verticalTolerance);

        int left =
            connected.Min(
                surface => surface.Left);

        int right =
            connected.Max(
                surface => surface.Right);

        return new Rectangle(
            left,
            origin.Top,
            right - left,
            1);
    }

    public Surface? FindSupportingSurfaceAt(
        Surface origin,
        double creatureX,
        int creatureWidth,
        double verticalTolerance)
    {
        double feetCenterX =
            creatureX +
            creatureWidth / 2.0;

        return GetConnectedWalkableSurfaces(
                origin,
                verticalTolerance)
            .Where(surface =>
                feetCenterX >= surface.Left &&
                feetCenterX <= surface.Right)
            .OrderBy(surface =>
                Math.Abs(
                    surface.Top -
                    origin.Top))
            .FirstOrDefault();
    }

    private List<Surface> GetConnectedWalkableSurfaces(
        Surface origin,
        double verticalTolerance)
    {
        const int horizontalTolerance = 2;

        var connected =
            new List<Surface>
            {
            origin
            };

        bool added;

        do
        {
            added = false;

            foreach (Surface candidate in
                     _surfaces)
            {
                if (connected.Contains(
                        candidate))
                {
                    continue;
                }

                if (Math.Abs(
                        candidate.Top -
                        origin.Top) >
                    verticalTolerance)
                {
                    continue;
                }

                bool touchesConnectedSurface =
                    connected.Any(
                        existing =>
                            candidate.Left <=
                                existing.Right +
                                horizontalTolerance &&
                            candidate.Right >=
                                existing.Left -
                                horizontalTolerance);

                if (!touchesConnectedSurface)
                    continue;

                connected.Add(
                    candidate);

                added = true;
            }
        }
        while (added);

        return connected;
    }

    private static bool IsTaskbarAutoHideEnabled()
    {
        var appBarData =
            new APPBARDATA
            {
                cbSize =
                    (uint)Marshal.SizeOf<
                        APPBARDATA>()
            };

        uint state =
            SHAppBarMessage(
                ABM_GETSTATE,
                ref appBarData);

        return
            (state & ABS_AUTOHIDE) != 0;
    }

    public IReadOnlyList<Rectangle>
        GetMonitorWorkingAreas()
    {
        return Forms.Screen.AllScreens
            .Select(screen =>
                ToDipRectangle(
                    screen.WorkingArea))
            .ToList();
    }

    public IReadOnlyList<Rectangle>
        GetMonitorBounds()
    {
        return Forms.Screen.AllScreens
            .Select(screen =>
                ToDipRectangle(
                    screen.Bounds))
            .ToList();
    }

    public bool IsPointOnDesktop(
        Point point)
    {
        return GetMonitorBounds()
            .Any(bounds =>
                point.X >= bounds.Left &&
                point.X < bounds.Right &&
                point.Y >= bounds.Top &&
                point.Y < bounds.Bottom);
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

    private static Rectangle ToDipRectangle(
        Rectangle pixelRect)
    {
        Window? mainWindow =
            System.Windows.Application.Current?.MainWindow;

        if (mainWindow is null)
            return pixelRect;

        var source =
            PresentationSource.FromVisual(
                mainWindow);

        if (source?.CompositionTarget is null)
            return pixelRect;

        var transform =
            source.CompositionTarget
                .TransformFromDevice;

        var topLeft =
            transform.Transform(
                new System.Windows.Point(
                    pixelRect.Left,
                    pixelRect.Top));

        var bottomRight =
            transform.Transform(
                new System.Windows.Point(
                    pixelRect.Right,
                    pixelRect.Bottom));

        return new Rectangle(
            (int)Math.Round(topLeft.X),
            (int)Math.Round(topLeft.Y),
            (int)Math.Round(
                bottomRight.X -
                topLeft.X),
            (int)Math.Round(
                bottomRight.Y -
                topLeft.Y));
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
    private const uint ABM_GETSTATE = 0x00000004;
    private const uint ABS_AUTOHIDE = 0x00000001;

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [DllImport(
        "shell32.dll",
        SetLastError = true)]
    private static extern uint SHAppBarMessage(
        uint dwMessage,
        ref APPBARDATA pData);

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