using Desktop_Creatures.Tools.Fonts;
using System.Windows;

namespace Desktop_Creatures
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //only use if font changes are made, otherwise comment out to avoid unnecessary generation
            //MagicalStandardFontTool.Generate();

            base.OnStartup(e);
        }
    }
}
