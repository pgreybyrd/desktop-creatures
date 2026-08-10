using Desktop_Creatures.Tools.Fonts;
using System.Windows;
using WpfBitmapFonts.Core.Fonts;
using System.IO;

namespace Desktop_Creatures
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //only use if font changes are made, otherwise comment out to avoid unnecessary generation
            //MagicalStandardFontTool.Generate();
            var magicalStandardDefinition = new FontDefinition
            {
                Name = "MagicalStandard",

                CharacterRows =
                [
                    "{}[]|",
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZbdfghjklpqy0123456789!£$&()@?/\\",
                    "#%i;*",
                    "acemnorstuvwxz+:",
                    "<>",
                    "=,",
                    "'`^~",
                    "_-."
                ],

                Baseline = 7,

                SpaceAdvance = 3,

                BaselineAdjustments = new()
                {
                    ['g'] = 2,
                    ['j'] = 1,
                    ['p'] = 2,
                    ['q'] = 2,
                    ['y'] = 2,

                    [','] = 1,

                    ['_'] = 1,
                    ['-'] = -2
                }
            };

#if DEBUG
            const bool regenerateFonts = false;

            if (regenerateFonts)
            {
                FontTool.Generate(magicalStandardDefinition);
            }
#endif

            base.OnStartup(e);

            string projectRoot = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\..\"));

            string fontPath = Path.Combine(
                projectRoot,
                "Assets",
                "Fonts",
                $"font-{magicalStandardDefinition.Name}.json");

            BitmapFont magicalStandard =
                BitmapFontLoader.Load(fontPath);

            BitmapFontRegistry.Register(
                magicalStandard.Name,
                magicalStandard,
                setAsDefault: true);
        }
    }
}
