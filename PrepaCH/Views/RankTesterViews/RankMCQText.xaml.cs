using System.Diagnostics;

namespace PrepaCH.Views.RankTesterViews;

public partial class RankMCQText : ContentView, IExercise
{
    int Correct;
    int Answer;

    public RankMCQText()
	{
		InitializeComponent();
	}

    public void OnAnswerChanged(object sender, CheckedChangedEventArgs e)
    {
        RadioButton btn = sender as RadioButton;

        if (btn.IsChecked)
        {
            Answer = int.Parse(btn.Value.ToString());
        }
    }

    public async Task<bool> Check()
    {
        await RankCard.Flip();

        await Task.Delay(1000);

        return Answer == Correct;
    }

    public void Reset()
    {
        A.IsChecked = false;
        B.IsChecked = false;
        C.IsChecked = false;
        D.IsChecked = false;

        RankCard.Flip(1, RankCard.FlipState.FRONT);
    }

    public void Setup(List<Rank> ranks)
    {
        Reset();

        Random r = new Random();
        //Select a random rank
        int answer = r.Next(0, ranks.Count() - 1);
        
        RankCard.SetRank(ranks[answer]);

        var selected = Utils.RandomSet(4, 0, ranks.Count - 1);

        A.Content = ranks[selected.ElementAt(0)].GetLocalizedLabel();
        B.Content = ranks[selected.ElementAt(1)].GetLocalizedLabel();
        C.Content = ranks[selected.ElementAt(2)].GetLocalizedLabel();
        D.Content = ranks[selected.ElementAt(3)].GetLocalizedLabel();

        //If a randomly selected one was already correct, then UI doesn't need to be updated
        for (int i = 0; i < selected.Count(); i++)
        {
            if(selected.ElementAt(i) == answer)
            {
                Correct = i;
                return;
            }
        }

        Trace.WriteLine($"Nothing already correct, so setting one answer at random to be correct");

        //Select random answer to be right answer
        Correct = r.Next(0, 3);

        var CorrentRank = ranks[answer];
        switch (Correct)
        {
            case 0:
                A.Content = CorrentRank.GetLocalizedLabel();
                break;
            case 1:
                B.Content = CorrentRank.GetLocalizedLabel();
                break;
            case 2:
                C.Content = CorrentRank.GetLocalizedLabel();
                break;
            case 3:
                D.Content = CorrentRank.GetLocalizedLabel();
                break;
        }
    }
}
