using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Desktop_Creatures.Windowing;

public sealed class ZOrderManager
{
    public enum WindowLayer
    {
        MainMenu = 0,
        Ecosystem = 1,
        ToolWindow = 2
    }

    private static readonly IntPtr HwndTopmost =
        new(-1);

    private static readonly IntPtr HwndNotTopmost =
        new(-2);

    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoActivate = 0x0010;

    private static readonly IntPtr HwndTop =
        IntPtr.Zero;

    private bool _menusAlwaysOnTop;
    private bool _ecosystemAlwaysOnTop;

    private bool _isApplying;

    private readonly Dictionary<
        WindowLayer,
        List<Window>> _windows =
            new()
            {
                [WindowLayer.MainMenu] = [],
                [WindowLayer.Ecosystem] = [],
                [WindowLayer.ToolWindow] = []
            };

    public void SetPolicy(
        bool ecosystemAlwaysOnTop,
        bool menusAlwaysOnTop)
    {
        _ecosystemAlwaysOnTop =
            ecosystemAlwaysOnTop;

        _menusAlwaysOnTop =
            menusAlwaysOnTop;

        Apply();
    }

    public void Register(
        Window window,
        WindowLayer layer)
    {
        var collection =
            _windows[layer];

        if (collection.Contains(window))
            return;

        collection.Add(window);

        window.SourceInitialized +=
            Window_SourceInitialized;

        window.Closed +=
            Window_Closed;

        Apply();
    }

    private void Window_SourceInitialized(
        object? sender,
        EventArgs e)
    {
        Apply();
    }

    private void Window_Closed(
        object? sender,
        EventArgs e)
    {
        if (sender is not Window window)
            return;

        foreach (var collection in
            _windows.Values)
        {
            collection.Remove(window);
        }

        window.SourceInitialized -=
            Window_SourceInitialized;

        window.Closed -=
            Window_Closed;

        Apply();
    }

    public void Apply()
    {
        if (_isApplying)
            return;

        _isApplying = true;

        try
        {
            SetLayerBand(
                WindowLayer.MainMenu,
                _menusAlwaysOnTop);

            SetLayerBand(
                WindowLayer.ToolWindow,
                _menusAlwaysOnTop);

            SetLayerBand(
                WindowLayer.Ecosystem,
                _ecosystemAlwaysOnTop);

            if (_menusAlwaysOnTop &&
                _ecosystemAlwaysOnTop)
            {
                EnforceTopmostHierarchy();
            }
        }
        finally
        {
            _isApplying = false;
        }
    }

    private void EnforceTopmostHierarchy()
    {
        List<IntPtr> tools =
            GetHandles(
                WindowLayer.ToolWindow);

        List<IntPtr> ecosystem =
            GetHandles(
                WindowLayer.Ecosystem);

        List<IntPtr> mainMenu =
            GetHandles(
                WindowLayer.MainMenu);

        IntPtr insertAfter =
            HwndTopmost;

        insertAfter =
            ChainWindows(
                tools,
                insertAfter);

        insertAfter =
            ChainWindows(
                ecosystem,
                insertAfter);

        ChainWindows(
            mainMenu,
            insertAfter);
    }

    private List<IntPtr> GetHandles(
        WindowLayer layer)
    {
        return _windows[layer]
            .Where(window =>
                window.IsVisible)
            .Select(window =>
                new WindowInteropHelper(window)
                    .Handle)
            .Where(handle =>
                handle != IntPtr.Zero)
            .ToList();
    }

    private static IntPtr ChainWindows(
        IEnumerable<IntPtr> handles,
        IntPtr insertAfter)
    {
        foreach (IntPtr handle in handles)
        {
            SetWindowPos(
                handle,
                insertAfter,
                0,
                0,
                0,
                0,
                SwpNoMove |
                SwpNoSize |
                SwpNoActivate);

            insertAfter =
                handle;
        }

        return insertAfter;
    }

    private void SetLayerBand(
        WindowLayer layer,
        bool topmost)
    {
        IntPtr insertAfter =
            topmost
                ? HwndTopmost
                : HwndNotTopmost;

        foreach (Window window in
            _windows[layer])
        {
            if (!window.IsVisible)
                continue;

            IntPtr handle =
                new WindowInteropHelper(window)
                    .Handle;

            if (handle == IntPtr.Zero)
                continue;

            SetWindowPos(
                handle,
                insertAfter,
                0,
                0,
                0,
                0,
                SwpNoMove |
                SwpNoSize |
                SwpNoActivate);
        }
    }

    private static List<IntPtr> GetVisibleHandles(
        IEnumerable<Window> windows)
    {
        var handles =
            new List<IntPtr>();

        foreach (Window window in windows)
        {
            if (!window.IsVisible)
                continue;

            IntPtr handle =
                new WindowInteropHelper(window)
                    .Handle;

            if (handle != IntPtr.Zero)
            {
                handles.Add(handle);
            }
        }

        return handles;
    }

    private static void SetBand(
        IEnumerable<IntPtr> handles,
        bool topmost)
    {
        IntPtr band =
            topmost
                ? HwndTopmost
                : HwndNotTopmost;

        foreach (IntPtr handle in handles)
        {
            SetWindowPos(
                handle,
                band,
                0,
                0,
                0,
                0,
                SwpNoMove |
                SwpNoSize |
                SwpNoActivate);
        }
    }

    private static void EnforceHierarchy(
        IReadOnlyList<IntPtr> ecosystem,
        IReadOnlyList<IntPtr> menus,
        bool topmost)
    {
        if (ecosystem.Count == 0)
            return;

        //
        // Build one explicit top -> bottom chain.
        //

        IntPtr insertAfter =
            topmost
                ? HwndTopmost
                : HwndTop;

        // Ecosystem goes first = highest.
        foreach (IntPtr handle in ecosystem)
        {
            SetWindowPos(
                handle,
                insertAfter,
                0,
                0,
                0,
                0,
                SwpNoMove |
                SwpNoSize |
                SwpNoActivate);

            insertAfter =
                handle;
        }

        // Menus are explicitly inserted below
        // the entire ecosystem chain.
        foreach (IntPtr handle in menus)
        {
            SetWindowPos(
                handle,
                insertAfter,
                0,
                0,
                0,
                0,
                SwpNoMove |
                SwpNoSize |
                SwpNoActivate);

            insertAfter =
                handle;
        }
    }

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint flags);
}