using Newtonsoft.Json;
using System.Diagnostics;

namespace PrepaCH.Views;

/**
 * Flip animation inspired from
 * https://trailheadtechnology.com/xamarin-forms-fancy-animations/
 * https://github.com/trailheadtechnology/FancyAnimations/blob/master/FacncyAnimations/FacncyAnimations/RotatePage.xaml.cs
 */

public class Rank //public readonly struct Rank
{
    //While not very ideal, we need to store each language version of the rank name here
    // as switching language on the fly isn't supported in MAUI
    // In most frameworks in general, changing Locale on the fly like that is also somewhat strange and requires some boilerplate / finicky code
    public string English { get; init; }
    public string French { get; init; }
    public string German { get; init; }
    public string Italian { get; init; }

    public string GetLocalizedLabel()
    {
        string lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        switch (lang)
        {
            case "de":
                return German;
            case "it":
                return Italian;
            case "fr":
            case "en":
            default:
                return French;
        }
    }

#nullable enable
    public string? FrenchAbbreviation { get; init; }
    public string? GermanAbbreviation { get; init; }
    public string? ItalianAbbreviation { get; init; }

    public string? GetLocalizedAbbreviation()
    {
        string lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        switch (lang)
        {
            case "de":
                return GermanAbbreviation;
            case "it":
                return ItalianAbbreviation;
            case "fr":
            case "en":
            default:
                return FrenchAbbreviation;
        }
    }
#nullable disable

    public string Image { get; init; }

    public Rank(string en, string fr, string de, string it, string img, string frabbr = null, string deabbr = null, string itabbr = null)
    {
        this.English = en;
        this.French = fr;
        this.German = de;
        this.Italian = it;

        this.FrenchAbbreviation = frabbr;
        this.GermanAbbreviation = deabbr;
        this.ItalianAbbreviation = itabbr;

        this.Image = img;
    }

    public static async Task<List<Rank>> LoadRanks()
    {
        try
        {
            Debug.WriteLine("Ting");
            using var stream = await FileSystem.OpenAppPackageFileAsync("Ranks.json");
            using var reader = new StreamReader(stream);

            Trace.WriteLine("Going to deserialize");

            List<Rank> ranks = JsonConvert.DeserializeObject<List<Rank>>(reader.ReadToEnd());

            Trace.WriteLine($"Ranks : {ranks.Count}");
            Trace.WriteLine(String.Join(", ", ranks));
            return ranks;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Failed to read ranks :");
            Debug.WriteLine(e);
            return new List<Rank>(0);
        }
    }

    public override string ToString() => $"({English} - {French} - {German} - {Italian} - {Image})";
}

public partial class RankCard : ContentView
{
    public enum FlipState
    {
        FRONT, BACK, UNDEF
    }

    public bool IsFlipped  { get; private set; } = false;
    public bool IsFlipping { get; private set; } = false;
    public bool IsAcceptingTapGestures = true;
    public const uint timeout = 1000;

    //Using 0 here crashes on windows https://github.com/dotnet/maui/issues/18420
    public async Task Flip(uint timeout = timeout, FlipState targetState = FlipState.UNDEF)
    {
        if ((targetState == FlipState.FRONT &&  !IsFlipped) || (targetState == FlipState.BACK && IsFlipped)){
            return;
        }
        IsFlipping = true;
        View top;
        View bottom;
        if (IsFlipped)
        {
            top = Back; bottom = Front;
        }
        else
        {
            top = Front; bottom = Back;
        }

        //top.IsVisible = true;
        //bottom.IsVisible = false;
        /*
        bottom.RotationY = -270;
        await top.RotateYTo(-90, timeout, Easing.SpringIn);
        top.IsVisible = false;
        bottom.IsVisible = true;
        await bottom.RotateYTo(-360, timeout, Easing.SpringOut);
        bottom.RotationY = 0;
        */
        IsFlipped = !IsFlipped;

        if (Front.IsVisible)
        {
            Back.RotationY = -270;
            await Front.RotateYTo(-90, timeout, Easing.SpringIn);
            Front.IsVisible = false;
            Back.IsVisible = true;
            await Back.RotateYTo(-360, timeout, Easing.SpringOut);
            Back.RotationY = 0;
        }
        else
        {
            Front.RotationY = -270;
            await Back.RotateYTo(-90, timeout, easing: Easing.SpringOut);
            Back.IsVisible = false;
            Front.IsVisible = true;
            await Front.RotateYTo(-360, timeout, Easing.SpringOut);
            Front.RotationY = 0;
        }

        IsFlipping = false;
    }

    /*
     * <VerticalStackLayout.GestureRecognizers>
			<TapGestureRecognizer Tapped="OnTapped" />
		</VerticalStackLayout.GestureRecognizers>
    */
    private void OnTapped(object sender, TappedEventArgs args)
    {
        if (!IsFlipping && IsAcceptingTapGestures)
            Flip();
    }

    public void SetRank(Rank rank)
    {
        Trace.WriteLine($"new source : Ranks/{rank.Image}.png");
        //Ensignia.Source = $"Ranks/{rank.Image}_CH.png";
        Ensignia.Source = ImageSource.FromFile($"{rank.Image}.png");
        Camo.Source = ImageSource.FromFile($"{rank.Image}_camo.png");
        German.Text = rank.German;
        French.Text = rank.French;
        Italian.Text = rank.Italian;

        if (rank.GermanAbbreviation != null && rank.FrenchAbbreviation != null && rank.ItalianAbbreviation != null)
        {
            /*GermanAbbreviation.IsVisible = true;
            FrenchAbbreviation.IsVisible = true;
            ItalianAbbreviation.IsVisible = true;*/
            GermanAbbreviation.Text = rank.GermanAbbreviation;
            FrenchAbbreviation.Text = rank.FrenchAbbreviation;
            ItalianAbbreviation.Text = rank.ItalianAbbreviation;
        } 
        else
        {
            /*GermanAbbreviation.IsVisible = false;
            FrenchAbbreviation.IsVisible = false;
            ItalianAbbreviation.IsVisible = false;*/
        }
    }

    public RankCard()
    {
        InitializeComponent();
    }

    //Doing Measure doesn't do anything...
    /*protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        double longestSide = Math.Max(widthConstraint, heightConstraint);
        //define minimum size of control
        var size = new Size((heightConstraint * 2.0) / 3.0, heightConstraint);
        System.Diagnostics.Debug.WriteLine($"MeasureOverride width={size.Width},height={size.Height}");
        this.DesiredSize = size;

        //base.MeasureOverride(size.Width, size.Height);

        return size;
    }*/

    /*protected override Size ArrangeOverride(Rect bounds) //bounds = { X=0, Y=0, Width=1013, Height=0 } 
    {
        System.Diagnostics.Debug.WriteLine($"ArrangeOverride width={bounds.Width},height={bounds.Height}");

        //this.WidthRequest = bounds.Width;
        //this.HeightRequest = bounds.Height;
        //this.Content.W

        BackGrid.WidthRequest = bounds.Width;
        BackGrid.HeightRequest = bounds.Height;
        Front.WidthRequest = bounds.Width;
        Front.HeightRequest = bounds.Height;

        BackGrid.Arrange(bounds);
        Front.Arrange(bounds); //results int Layout cycle detected.  Layout could not complete.

        //BackGrid.CrossPlatformArrange(bounds);
        //FrontGrid.CrossPlatformArrange(bounds);

        base.ArrangeOverride(bounds);

        return new Size(bounds.Width, bounds.Height);
    }*/
}