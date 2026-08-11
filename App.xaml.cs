using System.IO;
using System.Windows;
using System.Windows.Threading;
using WpfBitmapFonts.Core.Fonts;

namespace Desktop_Creatures
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(
            StartupEventArgs e)
        {
            DispatcherUnhandledException +=
                App_DispatcherUnhandledException;

            base.OnStartup(e);

            string fontPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Assets",
                    "Fonts",
                    "font-MagicalStandard.json");

            BitmapFont magicalStandard =
                BitmapFontLoader.Load(fontPath);

            BitmapFontRegistry.Register(
                magicalStandard.Name,
                magicalStandard,
                setAsDefault: true);
        }

        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            string crashPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "crash.txt");

            File.WriteAllText(
                crashPath,
                e.Exception.ToString());

            System.Windows.MessageBox.Show(
                e.Exception.ToString(),
                "Desktop Creatures crashed");

            e.Handled = false;
        }
    }
}
        //    public partial class App : System.Windows.Application
        //    {
        //        protected override void OnStartup(StartupEventArgs e)
        //        {
        //            //only use if font changes are made, otherwise comment out to avoid unnecessary generation
        //            //MagicalStandardFontTool.Generate();
        //            var magicalStandardDefinition = new FontDefinition
        //            {
        //                Name = "MagicalStandard",

        //                CharacterRows =
        //                [
        //                    "{}[]|",
        //                    "ABCDEFGHIJKLMNOPQRSTUVWXYZbdfghjklpqy0123456789!£$&()@?/\\",
        //                    "#%i;*",
        //                    "acemnorstuvwxz+:",
        //                    "<>",
        //                    "=,",
        //                    "'`^~",
        //                    "_-."
        //                ],

        //                Baseline = 7,

        //                SpaceAdvance = 3,

        //                BaselineAdjustments = new()
        //                {
        //                    ['g'] = 2,
        //                    ['j'] = 1,
        //                    ['p'] = 2,
        //                    ['q'] = 2,
        //                    ['y'] = 2,

        //                    [','] = 1,

        //                    ['_'] = 1,
        //                    ['-'] = -2
        //                }
        //            };

        //#if DEBUG
        //            const bool regenerateFonts = false;

        //            if (regenerateFonts)
        //            {
        //                FontTool.Generate(magicalStandardDefinition);
        //            }
        //#endif

        //            base.OnStartup(e);

        //            string projectRoot = Path.GetFullPath(
        //                Path.Combine(
        //                    AppContext.BaseDirectory,
        //                    @"..\..\..\"));

        //            string fontPath = Path.Combine(
        //                projectRoot,
        //                "Assets",
        //                "Fonts",
        //                $"font-{magicalStandardDefinition.Name}.json");

        //            BitmapFont magicalStandard =
        //                BitmapFontLoader.Load(fontPath);

        //            BitmapFontRegistry.Register(
        //                magicalStandard.Name,
        //                magicalStandard,
        //                setAsDefault: true);
        //        }
        //    }
    
