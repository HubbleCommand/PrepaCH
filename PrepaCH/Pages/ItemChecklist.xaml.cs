using PrepaCH.Resources.Strings;
using PrepaCH.Views;
using System.Diagnostics;

namespace PrepaCH.Pages;

public partial class ItemChecklist : ContentPage
{
    //Values here defaulting to false, will be found when put into UI...
    private List<ChecklistItem> Personal = new()
    {
        new ChecklistItem("Chk-Towel", 1),
        new ChecklistItem("Chk-Lock", 1),
    };

    private List<ChecklistItem> Service = new()
    {
        new ChecklistItem("Chk-ServiceBooklet", 1),
        new ChecklistItem("Chk-PersonalWeapon", 1),
        new ChecklistItem("Chk-Bayonet", 1),
        new ChecklistItem("Chk-CombatHelmet", 1),
        new ChecklistItem("Chk-UniformB", 1),
        new ChecklistItem("Chk-LegElastics", 2),
        new ChecklistItem("Chk-Beret", 1),
        new ChecklistItem("Chk-UniformA", 1),
        new ChecklistItem("Chk-NBCMask", 1),
        new ChecklistItem("Chk-NBCMaskGTHolder", 1),
        new ChecklistItem("Chk-GT", 1),
        new ChecklistItem("Chk-GreyBags", 4),
        new ChecklistItem("Chk-UniformABag", 1),
        new ChecklistItem("Chk-BootCleaningKit", 1),
        new ChecklistItem("Chk-RegulationBags", 2),
        new ChecklistItem("Chk-CombatGloves16", 1),
        new ChecklistItem("Chk-ThermalGloves17", 1),
        new ChecklistItem("Chk-Scarf", 1),
        new ChecklistItem("Chk-SleepingBag", 1),
        new ChecklistItem("Chk-CombatBag", 1),
        new ChecklistItem("Chk-CAT", 1),
        new ChecklistItem("Chk-PPI", 1),
        new ChecklistItem("Chk-MouthGuard", 1),
        new ChecklistItem("Chk-Caddie", 1),
        new ChecklistItem("Chk-Gamelle", 1),
        new ChecklistItem("Chk-Gourd", 1),
        new ChecklistItem("Chk-Backpack04", 1),
        new ChecklistItem("Chk-FunctionInsignia", 4),
        new ChecklistItem("Chk-MilitiaInsignia", 4),
        new ChecklistItem("Chk-PocketKnife08", 1),
        new ChecklistItem("Chk-HearingProtection", 1),
        new ChecklistItem("Chk-TShirt90", 5),
        new ChecklistItem("Chk-MarkingJersey", 1),
        new ChecklistItem("Chk-TShirt06", 2),
        new ChecklistItem("Chk-BonnetMarin", 1),
        new ChecklistItem("Chk-LongPants06", 1),
        new ChecklistItem("Chk-PolarVest06", 1),
        new ChecklistItem("Chk-Boxers06", 2),
        new ChecklistItem("Chk-Knaggis", 5),
        new ChecklistItem("Chk-CombatGlasses14", 1),
        new ChecklistItem("Chk-CombatBoots", 1),
        new ChecklistItem("Chk-Dress", 1),
        new ChecklistItem("Chk-Purse", 1),
    };

    public ItemChecklist()
    {
        InitializeComponent();
        //SourcePicker.SelectedIndexChanged += SourcePicker_SelectedIndexChanged;
        //SourcePicker.SelectedIndex = 0;
        ChecklistView.Items = Service;
    }

    private void SourcePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;
        
        Debug.WriteLine($"CompetitionShooting Language value {Strings.ResourceManager.GetString("CompetitionShooting")}");
        Debug.WriteLine($"Current Index {selectedIndex}");

        switch (selectedIndex)
        {
            case 0:
                ChecklistView.Items = Personal; break;
            case 1:
                ChecklistView.Items = Service; break;
        }
    }
}