using PrepaCH.Resources.Strings;

namespace PrepaCH.Views;

public class ChecklistItem
{
    public string Key { get; set; }
    public int Count { get; set; }
    public bool Checked { get; set; } = false;

    public ChecklistItem(string key, int count, bool @checked)
    {
        Key = key;
        Count = count;
        Checked = @checked;
    }

    public ChecklistItem(string key, int count)
    {
        Key = key;
        Count = count;
    }
}

public partial class ChecklistView : TableView
{
    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(List<ChecklistItem>), typeof(ContentView), new List<ChecklistItem>());

    public List<ChecklistItem> Items
    {
        get { return (List<ChecklistItem>)GetValue(ItemsProperty); }
        set
        {
            SetValue(ItemsProperty, value);
            Root.Clear();

            var ts = new TableSection();

            foreach (var item in value)
            {
                var sc = new SwitchCell
                {
                    //Text = $"x{item.Count} - {item.StringKey}",
                    Text = $"x{item.Count} - {Strings.ResourceManager.GetString(item.Key)}",
                    On = Preferences.Get(item.Key, false)
                };
                sc.OnChanged += (s, e) =>
                {
                    Preferences.Set(item.Key, e.Value);
                };
                ts.Add(sc);
            }

            Root.Add(ts);
        }
    }
}