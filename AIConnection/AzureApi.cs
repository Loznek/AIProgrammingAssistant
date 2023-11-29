using AIProgrammingAssistant.Commands.Exceptions;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace AIProgrammingAssistant.AIConnection
{
    public class AzureApi : IAIFunctions
    {
        OpenAIClient client;

        public AzureApi()
        {
            string keyString;
            if (AIProgrammingAssistantPackage.apiKey == null)
            {
                TextInputDialog.Show("OpenAI Api Key", "Enter the key of the API", "key", out keyString);
                AIProgrammingAssistantPackage.apiKey = keyString;
            }
            else
            {
                keyString = AIProgrammingAssistantPackage.apiKey;
            }
            client = new OpenAIClient(keyString);
        }
        public async Task<string> AskForFeedbackAsync(string codeFile, string selectedCode)
        {
            var conversationMessages = new List<ChatMessage>()
                {
                    new(ChatRole.System, $"There is a C# file: " + codeFile +
                     $"There will be a given a part of this file, and you will be asked to make an opinion about that code, and only that part of code"),
                    new(ChatRole.User, $"There is this given C# code snippet:" +
                    selectedCode +
                    $"What do you think about this code? Give me a little feedback about my code"),
                };
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-3.5-turbo",
            };
            foreach (ChatMessage chatMessage in conversationMessages)
            {
                chatCompletionsOptions.Messages.Add(chatMessage);
            }

            //ChatMessage message = null;
            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatMessage message = response.Value.Choices[0].Message;
                return "\r\n" + message.Content + "\r\n";
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode.Equals("invalid_api_key")) throw new InvalidKeyException();
                else throw new AIApiException(new string(ex.Message.TakeWhile(c => c != '\r').ToArray()));
            }
            catch (AggregateException ex)
            {
                throw new AIApiException(ex.InnerException.Message);
            }
        }

        public async Task<string> AskForOptimizedCodeAsync(string codeFile, string selectedCode)
        {
            var conversationMessages = new List<ChatMessage>()
                {
                   new(ChatRole.System, $"There is a C# file: " + codeFile +
                                        $"There will be a given a part of this file, and you will be ordered to optimize that part of code - and only that part, not the whole file." +
                                        $"You have the whole file to understand the context of the geven code, and to see the indentation of it."),
                   new(ChatRole.User,   $"Optimize this given C# code snippet:" +
                                        selectedCode +
                                        $"Your ansver should only contain the optimized version of the given code without any explanation. " +
                                        $"If you strongly believe that, the code part cannot be optomized your answer must be exactly the following: 'Code can't be optimized' "),

                };
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-3.5-turbo",
            };
            foreach (ChatMessage chatMessage in conversationMessages)
            {
                chatCompletionsOptions.Messages.Add(chatMessage);
            }

            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatMessage message = response.Value.Choices[0].Message;
                if (message.Content.Contains("Code can't be optimized")) throw new AIApiException(message.Content);
                return "\r\n" + message.Content + "\r\n";
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode.Equals("invalid_api_key")) throw new InvalidKeyException();
                else throw new AIApiException(new string(ex.Message.TakeWhile(c => c != '\r').ToArray()));
            }
            catch (AggregateException ex)
            {
                throw new AIApiException(ex.InnerException.Message);
            }
        }

        public async Task<List<string>> AskForQueryAsync(string humanQuery, string context, string schema)
        {
            var conversationMessages = new List<ChatMessage>()
                {

                    new(ChatRole.System, $"We have a database with the following data object files in a .net project what uses entity framework:\n" + schema),
                    new(ChatRole.System, $"For the dbContext we have the following file" + context ),
                    new(ChatRole.User, $"Write me a LINQ query, what satisfies the following human question: " + humanQuery +
                                       $"\n Your answer should only contain the code of the LINQ query without any explanation." +
                                       $"If you can't create the LINQ query because the human question doesn't make sense based on the database entites, your answer should be exactly the following: 'LINQ query can't be created'")
                };

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-3.5-turbo",
                Temperature = (float)0.4,
                ChoiceCount = 3,
            };
            foreach (ChatMessage chatMessage in conversationMessages)
            {
                chatCompletionsOptions.Messages.Add(chatMessage);
            }

            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(
                chatCompletionsOptions);

                var responses = response.Value.Choices.Select(c => "\n" + c.Message.Content).ToList();
                responses.Distinct().ToList();
                if (responses.Contains("LINQ query can't be created")) throw new AIApiException("LINQ query can't be created");
                return responses;
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode.Equals("invalid_api_key")) throw new InvalidKeyException();
                else throw new AIApiException(new string(ex.Message.TakeWhile(c => c != '\r').ToArray()));
            }
            catch (AggregateException ex)
            {
                throw new AIApiException(ex.InnerException.Message);
            }

        }

        public async Task<string> AskForTestCodeAsync(string code, string context, string nameSpace, string className)
        {
            var conversationMessages = new List<ChatMessage>()
                {
                    new(ChatRole.User, $"Write a whole testfile for the given C# code, using Microsoft.VisualStudio.TestTools.UnitTesting testpackage" + code +
                                       $"Your answer must contain only  the testfile's code without any explanation. The name of the testclass is '"+className+"' in the '" +nameSpace + "' namespace."+
                                       $"To provide you additional information I gave you the whole file, which the code snippet belongs to: " + context),
                };

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-3.5-turbo",
            };

            foreach (ChatMessage chatMessage in conversationMessages)
            {
                chatCompletionsOptions.Messages.Add(chatMessage);
            }

            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatMessage message = response.Value.Choices[0].Message;

                return "\r\n" + message.Content + "\r\n";
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode.Equals("invalid_api_key")) throw new InvalidKeyException();
                else throw new AIApiException(new string(ex.Message.TakeWhile(c => c != '\r').ToArray()));
            }
            catch (AggregateException ex)
            {
                throw new AIApiException(ex.InnerException.Message);
            }
        }




        public async Task<string> AskForVariableRevisionAsync(string selectedCode, string context)
        {
            var conversationMessages = new List<ChatMessage>()
                {
                    new(ChatRole.System, $"There is a C# file: " + context +
                     $"There will be a given a part of this file, and you will be asked to revision that code part regarding its variable names, considering a c# naming conventions and the logic of the code snippet."+
                     $"Pay attention that all variable names are expressive. Replace all the variable names, for which you have a better solution." +
                     $"Your anwer should be in that form: first you should write the whole corrected code without any explanation, and after that you should list the variable name changes: which name you replaces for what name" +
                     $"Between the corrected code and the list of the changes put a line with '####' text." +
                     $"So your answer must be like this: 'Only the Corrected code goes here without any explanation or introduction' + '####\n' + 'list of changes goes here'."+
                     $"Your answer must contain the whole code snippet without explanation, not just the changed variables."+
                     $"For example you must give back some result in this form, where a variable in the given snippet named 'Some_variablename': " +
                     $"int someVariableName = 0;" +
                     $"someVariable++;"+
                     $"####" +
                     $"Some_variablename -> someVariableName"+
                     $"I give you another example, where there are more than one variable to change: " +
                     $"string girlName = \"Bob\";" +
                     $"string boyName = \"Alice\";"+
                     $"####" +
                     $"grlname -> girlName"+
                     $"bname -> boyName" +
                     $"\n\n If you think there is no need to change any variable name, your answer should be exactly the following: 'I don't suggest any variable name change'"),
                    new(ChatRole.User, $"There is this given C# code snippet:" + selectedCode +
                                       $"Which variables should be renamed? Give me back the corrected code and the list of the changed variables!"),
                };
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-3.5-turbo",
            };
            foreach (ChatMessage chatMessage in conversationMessages)
            {
                chatCompletionsOptions.Messages.Add(chatMessage);
            }

            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatMessage message = response.Value.Choices[0].Message;
                if (message.Content.Contains("I don't suggest any variable name change")) throw new AIApiException(message.Content);
                return "\r\n" + message.Content + "\r\n";
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode.Equals("invalid_api_key")) throw new InvalidKeyException();
                else throw new AIApiException(new string(ex.Message.TakeWhile(c => c != '\r').ToArray()));
            }
            catch (AggregateException ex)
            {
                throw new AIApiException(ex.InnerException.Message);
            }
        }

    }


}
