using PrepaCH.Pages;

namespace PrepaCH
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("checklist", typeof(ItemChecklist));
            Routing.RegisterRoute("convocation", typeof(ConvocationDates));
            Routing.RegisterRoute("shooting", typeof(RangeDates));
            Routing.RegisterRoute("ranktester", typeof(RankTester));
        }
    }
}