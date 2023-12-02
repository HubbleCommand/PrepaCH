using CommunityToolkit.Mvvm.ComponentModel;
using PrepaCH.Resources.Strings;
using PrepaCH.Views;
using System.Diagnostics;

namespace PrepaCH.Pages;


//TODO use FlexLayout to better support landscape...
//TODO have different exercise types: have multiple choice already, but add:
// drag / drop rank images to rank name, inverse multiple choice (multiple images, decide for rank text)
//TODO for answers with multiple cards, add Border to card so that can highlight correct answer with green "highlight"

/**
 * Make 3 exercise types:
 * MCQ with rank image given (chiise 1 of 4 texts)
 * MCQ with ran name given (choose 1 of 4 images)
 * with 4 static cards, move 4 text options to them (dra-and-drop feature from 
 */
/*enum RankTesterState
{
    WAITING,
    PLAYED,
}

public partial class RankTesterViewModel : ObservableObject
{
    [ObservableProperty]
    int correctAnswers = 0;

    [ObservableProperty]
    int wrongAnswers = 0;
}*/

public partial class RankTester : ContentPage
{
    int CorrectAnswers = 0;
    int WrongAnswers = 0;

    IExercise CurrentIExercise;
    VisualElement CurrentExerciseView;

    List<Rank> ranks;
    public async Task LoadRanks()
    {
        ranks = await Rank.LoadRanks();
        
    }

    public RankTester()
	{
        InitializeComponent();

        ScoreLbl.Text = $"{Strings.Score} : {CorrectAnswers} / {CorrectAnswers + WrongAnswers}";
        CheckBtn.IsEnabled = false;

        //https://stackoverflow.com/questions/73403608/net-maui-c-sharp-background-task-continuewith-and-notification-event
        Task.Run(async () =>
        {
            ranks = await Rank.LoadRanks();

            Dispatcher.Dispatch(new Action(() =>
            {
                MCQText.TranslationX = -MCQText.Width;
                MCQImage.TranslationX = -MCQImage.Width;
                DragAndMatch.TranslationX = -DragAndMatch.Width;

                ActivityIndicator.IsRunning = false;

                SetupNextExercise();
            }));
            
        });
        /*LoadRanks().ContinueWith((task) =>
        {
            //First, make all exercises invisible and move them away
            // (don't need to make invisible, they are in XML)
            MCQText.TranslationX = -MCQText.Width;
            MCQImage.TranslationX = -MCQImage.Width;
            DragAndMatch.TranslationX = -DragAndMatch.Width;

            SetupNextExercise();
        }, TaskScheduler.FromCurrentSynchronizationContext());*/
    }

    public async void SetupNextExercise()
    {
        if (CurrentExerciseView != null)
        {
            await CurrentExerciseView.TranslateTo(CurrentExerciseView.Width, 0, 1000, Easing.SpringIn);
            CurrentExerciseView.IsVisible = false;
            await CurrentExerciseView.TranslateTo(-CurrentExerciseView.Width, 0, 1000);
            CurrentExerciseView.IsVisible = true;
        }

        Random r = new Random();
        int rInt = r.Next(0, 3);

        switch (rInt)
        {
            case 0:
                CurrentIExercise = (IExercise)MCQText;
                CurrentExerciseView = MCQText;
                break;
            case 1:
                CurrentIExercise = (IExercise)MCQImage;
                CurrentExerciseView = MCQImage;
                break;
            case 2:
                CurrentIExercise = (IExercise)DragAndMatch;
                CurrentExerciseView = DragAndMatch;
                break;
        }

        CurrentIExercise.Setup(ranks);
        await CurrentExerciseView.TranslateTo(0, 0, 1000, Easing.SpringOut);

        //Hide animation show strings behind the stage
        //ExcerciseHolder.IsVisible = false;

        //Randomly choose a random exercise

        /*View excercise = new RankMCQText();

        ExcerciseHolder.Content = excercise;
        ((IExercise)excercise).Setup(ranks);*/

        //ExcerciseHolder.Content.TranslationX = ExcerciseHolder.Content.Width;

        //ExcerciseHolder.IsVisible = true;
        //Now slide child in
        //await ExcerciseHolder.Content.TranslateTo(0, 0, 1000, Easing.SpringOut);
        
        CheckBtn.IsEnabled = true;
    }

    public async void CheckAnswer(object sender, EventArgs e)
    {
        CheckBtn.IsEnabled = false;
        IExercise exercise = CurrentExerciseView as IExercise;

        bool correct = await exercise.Check();

        await Task.Delay(1000);

        String Message;
        if (correct)
        {
            CorrectAnswers++;
            Trace.WriteLine("Correct");
            Message = "Correct";
        }
        else
        {
            WrongAnswers++;
            Trace.WriteLine("WRONG");
            Message = "Wrong";
        }

        ScoreLbl.Text = $"{Strings.Score} : {CorrectAnswers} / {CorrectAnswers + WrongAnswers}";

        await DisplayAlert("Alert", Message, "OK");
        
        SetupNextExercise();
    }
}