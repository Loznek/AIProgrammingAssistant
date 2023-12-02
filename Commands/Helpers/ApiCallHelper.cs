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
                TextInputDialog.Show("Wrong OpenAI Api key was given", "You can change your API key", "key", out string keyString);
                AIProgrammingAssistantPackage.apiKey = keyString;
                return null;
            }
            catch (AIApiException apiException)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", apiException.Message);
                return null;
            }

            return apiAnswer;
        }

        
        /*public static async Task<List<string>> HandleApiCallForListAsync(Func<Task<List<string>>> calledFunction)
        {
            List<string> apiAnswer;
            try
            {
                apiAnswer = await calledFunction.Invoke();
            }
            catch (InvalidKeyException keyException)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", keyException.Message);
                TextInputDialog.Show("Wrong OpenAI Api key was given", "You can change your API key", "key", out string keyString);
                AIProgrammingAssistantPackage.apiKey = keyString;
                return null;
            }
            catch (AIApiException apiException)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", apiException.Message);
                return null;
            }

            return apiAnswer;
        }*/
    }
}
