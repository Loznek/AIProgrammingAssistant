using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.AIConnection
{
    public interface IAIFunctions
    {
        Task<string> AskForOptimizedCodeAsync(string codeFile, string selectedCode);
        Task<string> AskForTestCodeAsync(string code, string wholeCode, string nameSpace, string className);

        Task<List<string>> AskForQueryAsync(string humanQuery, string context, string schema);

        Task<string> AskForFeedbackAsync(string code, string context);

        Task<string> AskForVariableRevisionAsync(string code, string context);
    }
}
