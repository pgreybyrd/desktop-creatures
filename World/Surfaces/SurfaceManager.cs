using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

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
        foreach (var screen in Forms.Screen.AllScreens)
        {
            var area = screen.Bounds;

            //System.Windows.MessageBox.Show(
                //$"SCREEN Bounds L={area.Left} T={area.Top} R={area.Right} B={area.Bottom} W={area.Width} H={area.Height}");
            Debug.WriteLine(
                $"SCREEN Bounds L={area.Left} T={area.Top} R={area.Right} B={area.Bottom} W={area.Width} H={area.Height}");

            var dipArea = ToDipRectangle(screen.WorkingArea);

            _surfaces.Add(new Surface(
                new Rectangle(
                    dipArea.Left,
                    dipArea.Bottom - 1,
                    dipArea.Width,
                    1),
                "MonitorGround"));
        }
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

            if (IsIconic(hWnd))
                return true;

            GetWindowRect(hWnd, out RECT rect);

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

    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}