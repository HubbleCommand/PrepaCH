namespace PrepaCH.Services
{
    /**
     * Allows displaying Popups https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups from anywhere in the app (notably ViewModels)
     * based on https://stackoverflow.com/questions/72429055/how-to-displayalert-in-a-net-maui-viewmodel
     */
    public interface IPopupService
    {
        /**
         * Popup for information
         */
        public Task Alert(string title, string message, string cancel = "Ok");

        /**
         * Popup for confirmation
         */
        public Task<bool> Alert(string title, string message, string accept = "Ok", string cancel = "No");

        /**
         * Popup with a single Entry for user-enterable text
         */
        public Task<string> Prompt(string title, string message, string accept = "Yes", string cancel = "Cancel", string placeholder = default, int maxLength = -1, Microsoft.Maui.Keyboard keyboard = default, string initialValue = "");
        
        /**
         * Popup with signle-choice list
         */
        public Task<string> Sheet(string title, string cancel, string destruction, Microsoft.Maui.FlowDirection flowDirection = default, params string[] buttons);
    }

    public class PopupService : IPopupService
    {
        public Task Alert(string title, string message, string cancel = "Ok")
        {
            return Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public Task<bool> Alert(string title, string message, string accept = null, string cancel = "Ok")
        {
            return Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        public Task<string> Prompt(string title, string message, string accept = "Yes", string cancel = "Cancel", string placeholder = default, int maxLength = -1, Keyboard keyboard = default, string initialValue = "")
        {
            return Application.Current.MainPage.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue);
        }

        public Task<string> Sheet(string title, string cancel, string destruction, FlowDirection flowDirection = default, params string[] buttons)
        {
            return Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, flowDirection, buttons);
        }
    }
}
