using AIProgrammingAssistant.Commands.Exceptions;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Helpers
{
    internal static class ApiCallHelper
    {
        /// <summary>
        /// Handles the api call and shows the user the appropriate message box if an error occured
        /// </summary>
        public static async Task<T> HandleApiCallAsync<T>(Func<Task<T>> calledFunction) where T : class
        {
            T apiAnswer;
            try
            {
                apiAnswer = await calledFunction.Invoke();
            }
            catch (InvalidKeyException keyException)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", keyException.Message);
                bool userInsertedKey=TextInputDialog.Show("Wrong OpenAI Api key was given", "You can change your API key", "key", out string keyString);

                if (!userInsertedKey) AIProgrammingAssistantPackage.ApiKey = null;
                else AIProgrammingAssistantPackage.ApiKey = keyString;

                return null;
            }
            catch (AIApiException apiException)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", apiException.Message);
                return null;
            }

            return apiAnswer;
        }
    }
}
