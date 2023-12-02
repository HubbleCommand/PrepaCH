using System.Diagnostics;
using System.Security.AccessControl;

namespace PrepaCH.Views.RankTesterViews;

public partial class RankMCQImage : ContentView, IExercise
{
    public enum OptionABCD { A, B, C, D }

    int Correct;
    int Answer;
    bool IsChecking = false;

    //Styling info for Border elements
    private const double SelectedStrokeThickness = 4.0;
    private const double UnselectedStrokeThickness = 0.0;


    private Border? CurrentlySelectedBorder = null;

    public RankMCQImage()
	{
		InitializeComponent();
	}

    private void ToggleBorderUIState(Border border, bool selected)
    {
        border.StrokeThickness = selected ? SelectedStrokeThickness : UnselectedStrokeThickness;
    }

    private void OnCardTapped(object sender, TappedEventArgs args)
    {
        if (IsChecking)
            return;

        Trace.WriteLine($"Card Tapped: {sender.ToString()}");

        if (CurrentlySelectedBorder != null)
        {
            ToggleBorderUIState(CurrentlySelectedBorder, false);

            if (CurrentlySelectedBorder == sender)
            {
                CurrentlySelectedBorder = null;
                return;
            }
        }

        Border border = sender as Border;
        ToggleBorderUIState(border, true);
        CurrentlySelectedBorder = border;

        if (args.Parameter != null)
        {
            Trace.WriteLine($"parameter: {args.Parameter}, type: {args.Parameter.GetType()}");
            if (args.Parameter.GetType() == typeof(string))
            {
                Answer = int.Parse((string)args.Parameter);
            }
        }
    }

    public async Task<bool> Check()
    {
        IsChecking = true;
        //First, flip user answer
        switch (Answer)
        {
            case 0:
                await RankCardA.Flip();
                break;
            case 1:
                await RankCardB.Flip();
                break;
            case 2:
                await RankCardC.Flip();
                break;
            case 3:
                await RankCardD.Flip();
                break;
        }

        //If correct, return now
        if (Answer == Correct)
        {
            return true;
        }

        //If incorrect, flip correct card
        switch (Correct)
        {
            case 0:
                await RankCardA.Flip();
                break;
            case 1:
                await RankCardB.Flip();
                break;
            case 2:
                await RankCardC.Flip();
                break;
            case 3:
                await RankCardD.Flip();
                break;
        }

        return Answer == Correct;
    }

    public void Reset()
    {
        IsChecking = false;
        /*A.IsChecked = false;
        B.IsChecked = false;
        C.IsChecked = false;
        D.IsChecked = false;*/

        RankCardA.Flip(1, RankCard.FlipState.FRONT);
        RankCardB.Flip(1, RankCard.FlipState.FRONT);
        RankCardC.Flip(1, RankCard.FlipState.FRONT);
        RankCardD.Flip(1, RankCard.FlipState.FRONT);

        if (CurrentlySelectedBorder != null)
        {
            ToggleBorderUIState(CurrentlySelectedBorder, false);
        }
    }

    public void Setup(List<Rank> ranks)
    {
        Reset();

        Random r = new Random();
        //Select a random rank
        int answer = r.Next(0, ranks.Count() - 1);

        //Set label
        string lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        switch (lang)
        {
            case "fr":
            case "en":
                TargetAnswer.Text = ranks[answer].French;
                break;
            case "de":
                TargetAnswer.Text = ranks[answer].German;
                break;
            case "it":
                TargetAnswer.Text = ranks[answer].Italian;
                break;
        }

        //Set choices
        var selected = Utils.RandomSet(4, 0, ranks.Count - 1);

        RankCardA.SetRank(ranks[selected.ElementAt(0)]);
        RankCardB.SetRank(ranks[selected.ElementAt(1)]);
        RankCardC.SetRank(ranks[selected.ElementAt(2)]);
        RankCardD.SetRank(ranks[selected.ElementAt(3)]);

        //If a randomly selected one was already correct
        for (int i = 0; i < selected.Count(); i++)
        {
            if (selected.ElementAt(i) == answer)
            {
                Correct = i;
                return;
            }
        }

        var CorrentRank = ranks[answer];
        switch (Correct)
        {
            case 0:
                RankCardA.SetRank(CorrentRank);
                break;
            case 1:
                RankCardB.SetRank(CorrentRank);
                break;
            case 2:
                RankCardC.SetRank(CorrentRank);
                break;
            case 3:
                RankCardD.SetRank(CorrentRank);
                break;
        }
    }
}