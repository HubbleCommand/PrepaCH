using System.Diagnostics;

namespace PrepaCH.Views.RankTesterViews;

public partial class RankDragAndMatch : ContentView, IExercise
{
    string[] Correct;
    string[] Answers;

    public RankDragAndMatch()
	{
		InitializeComponent();

        Correct = new string[4];
        Answers = new string[4];
    }

    //Basic D&D from
    // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/drag-and-drop?view=net-maui-8.0&tabs=windows
    // https://stackoverflow.com/questions/71402699/drag-and-drop-net-maui

    private void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs args)
    {
        var label = (sender as Element)?.Parent as Label;
        args.Data.Properties.Add("Text", label.Text);
    }

    private void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
    {
        var data = e.Data.Properties["Text"].ToString();
        var border = (sender as Element)?.Parent as Border;
        border.Content = new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
            Text = data
        };
    }

    public async Task<bool> Check()
    {
        bool HadWrong = false;

        string AnswerA = (DropTargetA.Content as Label).Text;
        string AnswerB = (DropTargetB.Content as Label).Text;
        string AnswerC = (DropTargetC.Content as Label).Text;
        string AnswerD = (DropTargetD.Content as Label).Text;

        //TODO
        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/animation/custom?view=net-maui-8.0
        // https://www.youtube.com/watch?v=bS0N6U-Ei78
        if (Correct[0] != AnswerA)
        {
            Trace.WriteLine($"Wrong answer for 0 : {Correct[0]}, got: {AnswerA}");
            RankCardBorderA.Stroke = Colors.Red;
            HadWrong = true;
        } else
        {
            RankCardBorderA.Stroke = Colors.Green;
        }

        if (Correct[1] != AnswerB)
        {
            Trace.WriteLine($"Wrong answer for 1 : {Correct[1]}, got: {AnswerB}");
            RankCardBorderA.Stroke = Colors.Red;
            HadWrong = true;
        }
        else
        {
            RankCardBorderB.Stroke = Colors.Green;
        }

        if (Correct[2] != AnswerC)
        {
            Trace.WriteLine($"Wrong answer for 2 : {Correct[2]}, got: {AnswerC}");
            RankCardBorderC.Stroke = Colors.Red;
            HadWrong = true;
        }
        else
        {
            RankCardBorderC.Stroke = Colors.Green;
        }

        if (Correct[3] != AnswerD)
        {
            Trace.WriteLine($"Wrong answer for 3 : {Correct[3]}, got: {AnswerD}");
            RankCardBorderD.Stroke = Colors.Red;
            HadWrong = true;
        }
        else
        {
            RankCardBorderD.Stroke = Colors.Green;
        }

        return !HadWrong;
    }

    public void Reset()
    {
        //Can't set content to null for windows
        //https://github.com/dotnet/maui/pull/12634
        DropTargetA.Content = new Label { Text = string.Empty };
        DropTargetB.Content = new Label { Text = string.Empty };
        DropTargetC.Content = new Label { Text = string.Empty };
        DropTargetD.Content = new Label { Text = string.Empty };

        RankCardBorderA.Stroke = Color.FromUint(0x00000000);
        RankCardBorderB.Stroke = Color.FromUint(0x00000000);
        RankCardBorderC.Stroke = Color.FromUint(0x00000000);
        RankCardBorderD.Stroke = Color.FromUint(0x00000000);
    }

    public void Setup(List<Rank> ranks)
    {
        Reset();
        var selected = Utils.RandomSet(4, 0, ranks.Count);

        RankCardA.SetRank(ranks[selected.ElementAt(0)]);
        RankCardB.SetRank(ranks[selected.ElementAt(1)]);
        RankCardC.SetRank(ranks[selected.ElementAt(2)]);
        RankCardD.SetRank(ranks[selected.ElementAt(3)]);

        Correct[0] = ranks[selected.ElementAt(0)].GetLocalizedLabel();
        Correct[1] = ranks[selected.ElementAt(1)].GetLocalizedLabel();
        Correct[2] = ranks[selected.ElementAt(2)].GetLocalizedLabel();
        Correct[3] = ranks[selected.ElementAt(3)].GetLocalizedLabel();

        //randomize text order
        var labelOrder = Utils.RandomSet(4, 0, 4);
        LabelA.Text = ranks[selected.ElementAt(labelOrder.ElementAt(0))].GetLocalizedLabel();
        LabelB.Text = ranks[selected.ElementAt(labelOrder.ElementAt(1))].GetLocalizedLabel();
        LabelC.Text = ranks[selected.ElementAt(labelOrder.ElementAt(2))].GetLocalizedLabel();
        LabelD.Text = ranks[selected.ElementAt(labelOrder.ElementAt(3))].GetLocalizedLabel();
    }
}