using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace PrepaCH.Pages;

public class Root
{
    public string statusCode { get; set; }
    public List<Unit> units { get; set; }
    public Infos infos { get; set; }
}

public class Infos
{
    public string lastUpdate { get; set; }
}

public class Unit : ObservableObject
{
    [JsonProperty("unit")]
    public string Name { get; set; }
    [JsonProperty("from")]
    public string From { get; set; }
    [JsonProperty("until")]
    public string Until { get; set; }
    /*[JsonProperty("comment")]
    public string Comment { get; set; }*/

    public override string ToString()
    {
        //return $"{{Name:{Name}, From:{From}, Until:{Until}, Comment:{Comment}}}";
        return $"{{Name:{Name}, From:{From}, Until:{Until}}}";
    }
}

public partial class ConvocationDatesViewModel : ObservableObject
{
    public ConvocationDatesViewModel()
    {
        //Check if already has UnitName in preferences (and others?)
        unitName = Preferences.Default.Get("unit", "");

        Units.CollectionChanged += Units_CollectionChanged;
    }

    [ObservableProperty]
    public ObservableCollection<Unit> units = new ObservableCollection<Unit>();
    //public ObservableCollection<Unit> Units = new();

    [ObservableProperty]
    public string unitName;

    DateTime from = new(DateTime.Now.Year, 1, 1);
    public DateTime From { 
        get
        {
            return from;
        }
        set 
        { 
            Debug.WriteLine($"From - {value}");
            from = value;
        } 
    }

    [ObservableProperty]
    public DateTime to = new(DateTime.Now.Year, 12, 31);
    //public DateTime to { get; set; } = new DateTime(DateTime.Now.Year, 12, 31);

    [ObservableProperty]
    public bool isSearching = false;

    [RelayCommand]
    private void ItemSelected(object item)
    {
        //do something with the item, e.g. cast it to Category
        Debug.WriteLine($"{item.ToString()} is selected");
    }

    [RelayCommand]
    public async Task Search()
    {
        if (IsSearching)    //Shouldn't be possible...
        {
            return;
        }

        IsSearching = true;
        Units.Clear();
        Debug.WriteLine($"UnitName - {UnitName}");
        Preferences.Default.Set("unit", UnitName);
        //https://www.vtg.admin.ch/content/vtg-internet/fr/mein-militaerdienst/aufgebotsdaten/jcr:content/contentPar/wkdata.wksearch.json?_charset_=UTF-8&language=fr&query=cp%20em%20br%20mec%201&from=01.01.2023&until=31.12.2023


        //URL is async follows:
        //base URL + random query shit
        //https://www.vtg.admin.ch/content/vtg-internet/fr/mein-militaerdienst/aufgebotsdaten/jcr:content/contentPar/wkdata.wksearch.json?_charset_=UTF-8
        //language
        //&language=fr
        //unit name search, spaces are %20
        //&query=cp%20em%20br%20mec%201
        //From & to dates
        //&from=01.01.2023
        //&until=31.12.2023
        //Result is a json of format:
        //{"statusCode":"200","apiVersion":"1","lang":"fr","numberOfUnits":"2","meta":{"labels":{"unit":"Unité","from":"du","until":"au","comment":"Remarques"}},"units":[{"unit":"Cp EM br méc 1","from":"09.10.2023","until":"27.10.2023","comment":null},{"unit":"Cp EM br méc 11","from":"04.09.2023","until":"22.09.2023","comment":null}],"infos":{"lastUpdate":"30.10.2023"}}
        //Useful for converting JSON to C# https://json2csharp.com/

        HttpClient _client = new HttpClient();
        JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        //TODO change language param based on System.Globalization.CultureInfo.CurrentCulture
        Debug.WriteLine($"Language - {System.Globalization.CultureInfo.CurrentCulture.Name}");
        Debug.WriteLine($"Language - {System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName}");
        Uri uri = new Uri(string.Format(
            "https://www.vtg.admin.ch/content/vtg-internet/fr/mein-militaerdienst/aufgebotsdaten/jcr:content/contentPar/wkdata.wksearch.json?_charset_=UTF-8" +
            "&language=fr" +    //Doesn't matter, we don't use this...
            "&query=" + Uri.EscapeDataString(UnitName) +
            $"&from={From.Day}.{From.Month}.{From.Year}" +
            $"&until={To.Day}.{To.Month}.{To.Year}",
            string.Empty
        ));
        Debug.WriteLine($"Query = {uri.Query}");
        try
        {
            HttpResponseMessage response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Root root = JsonConvert.DeserializeObject<Root>(content);
                Debug.WriteLine(@"Root {0}", root);
                Debug.WriteLine(@"Root {0}", root.units[0].Name);
                Debug.WriteLine(@"Units {0}", Units);
                Debug.WriteLine(@"Units {0}", Units.Count);

                if (root == null)
                {
                    IsSearching = false;
                    return;
                }

                if (root.units == null)
                {
                    IsSearching = false;
                    return;
                }

                foreach (Unit item in root.units)
                {
                    Debug.WriteLine(@"Unit {0}", item.ToString());
                    Debug.WriteLine(@"Units {0}", Units);
                    Debug.WriteLine(@"Units {0}", Units.Count);
                    Units.Add(item);
                }
                IsSearching = false;
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }
        Units.Clear();
        IsSearching = false;
    }

    private void Units_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine("COLLECTION CHANGED");
    }
}

public partial class ConvocationDates : ContentPage
{
    public ConvocationDates()
	{
		InitializeComponent();
	}
}