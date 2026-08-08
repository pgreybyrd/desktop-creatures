using Desktop_Creatures.Tools.Fonts;
using System.Windows;
using Desktop_Creatures.Rendering.Fonts;
using System.IO;

namespace Desktop_Creatures
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //only use if font changes are made, otherwise comment out to avoid unnecessary generation
            //MagicalStandardFontTool.Generate();

            base.OnStartup(e);

            string projectRoot = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));

            string fontPath = Path.Combine(
                projectRoot,
                "Assets",
                "Fonts",
                "font-MagicalStandard.json");

            BitmapFont magicalStandard =
                BitmapFontLoader.Load(fontPath);

            BitmapFontRegistry.Register(
                "MagicalStandard",
                magicalStandard,
                setAsDefault: true);
        }
    }
}
