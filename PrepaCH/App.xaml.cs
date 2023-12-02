using PrepaCH.Services;

namespace PrepaCH
{
    public partial class App : Application
    {
        public static IServiceProvider Services;
        public static IPopupService PopupService;

        public App(IServiceProvider provider)
        {
            InitializeComponent();

            Services = provider;
            PopupService = Services.GetService<IPopupService>();

            MainPage = new AppShell();
        }
    }
}