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
            if (AIProgrammingAssistantPackage.ApiKey == null)
            {
                bool inserted = TextInputDialog.Show("OpenAI Api Key", "Enter the key of the API", "key", out keyString);
                if (inserted) AIProgrammingAssistantPackage.ApiKey = keyString;
                else 
                {
                    keyString = "empty key";
                    AIProgrammingAssistantPackage.ApiKey = keyString;
                }

            }
            else
            {
                keyString = AIProgrammingAssistantPackage.ApiKey;
            }
            client = new OpenAIClient(keyString);
        }
        public async Task<string> AskForFeedbackAsync(string codeFile, string selectedCode)
        {
            var conversationMessages = new List<ChatMessage>()
                {
                    new(ChatRole.System, $"There is a C# file: " + codeFile +
                     $"The user will give you a part of this file, and you will be asked to make an opinion about that code, and only that part of code" +
                     $"Your feedback could be based on security, clean code or performance attributes of the code." +
                     $"Your feedback should be maximum 6 sentence long, with every sentence in a new line. "
                     ),
                    new(ChatRole.User, $"There is this given C# code snippet:" +
                    selectedCode +
                    $"What do you think about this code? Give me a feedback  about my code!"),
                };
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-4",
                Temperature = (float)0.8,
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

        public async Task<string> AskForOptimizedCodeAsync(string codeFile, string selectedCode)
        {
            var conversationMessages = new List<ChatMessage>()
                {
                   new(ChatRole.System, $"There is a C# file: " + codeFile +
                                        $"The user will give you a part of this file, and you will be ordered to optimize that part of code - and only that part, not the whole file." +
                                        $" Feel free to optimize the performance or code simplicty of the code" +
                                        $"You have the whole file to understand the context of the given code." +
                                        $"If you think that, the given code doesn't make sense or cannot be more effective, your answer must be exactly the following:" +
                                        $" 'Code can't be optimized' and nothing else, but your main goal is to optimize the snippet\n"),
                   new(ChatRole.User,   $"Optimize this given C# code snippet:" +
                                        selectedCode +
                                        $"Your answer should only contain the optimized version of the given code without any explanation. ")


                };
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-4",
                Temperature = (float)0.3,
            };
            foreach (ChatMessage chatMessage in conversationMessages)
            {
                chatCompletionsOptions.Messages.Add(chatMessage);
            }

            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatMessage message = response.Value.Choices[0].Message;
                if (message.Content.Contains("Code can't be optimized")) throw new AIApiException("\r\n" + message.Content + "\r\n");
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

        public async Task<List<string>> AskForQueryAsync(string humanQuery,string wholeCode, string context, string schema)
        {
            var conversationMessages = new List<ChatMessage>()
                {

                    new(ChatRole.System, $"We have a database with the following entity files in a .NET project what uses Entity Framework:\n" + schema +
                                         $"\n For the Database Context we have the following file: \n" + context +
                                         $"\n There will be a given a human question, and you will be asked to write a LINQ query, which satisfies the question.\n" +
                                         $"\n This is the code file where the query will be inserted:\n" + wholeCode +
                                         $"\n Your answer must only contain the code of the LINQ query without any explanation or other characters." +
                                         $"If you can't create the LINQ query because the human question doesn't make sense based on the database entites, your answer must be exactly the following: 'LINQ query can't be created'"),
                    new(ChatRole.User, $"Write me a LINQ query, what satisfies the following human question:\n" + humanQuery +
                                       $"Your answer must only contain the code of the LINQ query without any explanation or other characters.")

                                      
                                       
                };

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-4",
                Temperature = (float)1.1,
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
                responses=responses.Distinct().ToList();
                if (responses.Contains("\nLINQ query can't be created")) throw new AIApiException("LINQ query can't be created");
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
                                       $"Try to cover all edge cases and test exception paths!"+
                                       $"If you think that, the given code doesn't make sense or cannot be tested, your answer must be exactly the following: 'Code can't be tested'"+
                                       $"To provide you additional information I gave you the whole file, which the code snippet belongs to: " + context),
                };

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-4",
                Temperature = (float)0.4,
            };

            foreach (ChatMessage chatMessage in conversationMessages)
            {
                chatCompletionsOptions.Messages.Add(chatMessage);
            }

            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatMessage message = response.Value.Choices[0].Message;
                if (message.Content.Contains("Code can't be tested")) throw new AIApiException("Code can't be tested");
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
                     $"The user will give you a part of this file, and you will be asked to revision that code part regarding its variable names, considering c# naming conventions and the logic of the code snippet."+
                     $"Pay attention that all variable names should be expressive. Replace all the variable and even declarative function names, for which you have a better solution." +
                     $"Your anwer should be in that form: first you must write the whole corrected code without any explanation, and after that you should list the variable name changes: which name you have replaced for what new name" +
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
                     $"\n\n If you think there is no need to change any variable name, your answer must be exactly the following: 'I don't suggest any variable name change'"),
                    new(ChatRole.User, $"There is this given C# code snippet:" + selectedCode +
                                       $"Which variables should be renamed? Give me back the corrected code and the list of the changed variables!"),
                };
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-4",
                Temperature = (float)1.2,
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
