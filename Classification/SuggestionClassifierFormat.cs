using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace AIProgrammingAssistant.Classification
{




    /// <summary>
    /// The class describes the formating rules for the "optimization" classification type
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "suggestion.optimization")]
    [Name("suggestion.optimization")]
    [Order(Before = Priority.High, After = Priority.High)]
    internal sealed class SuggestionOptimizationFormat : ClassificationFormatDefinition
    {
        public SuggestionOptimizationFormat()
        {
            ForegroundColor = Colors.Crimson;
            IsItalic = true;
            this.TextDecorations = null;
            this.TextEffects = null;
            BackgroundOpacity = 0;
            ForegroundOpacity = 1;
        }
    }

    /// <summary>
    /// The class describes the formating rules for the "linq" classification type
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "suggestion.linq")]
    [Name("suggestion.linq")]
    [Order(Before = Priority.High, After = Priority.High)]
    internal sealed class SuggestionLinqFormat : ClassificationFormatDefinition
    {
        public SuggestionLinqFormat()
        {
            ForegroundColor = Colors.DarkOrange;
            IsBold = true;
            IsItalic = true;
            BackgroundOpacity = 0;
            this.TextDecorations = null;
            this.TextEffects = null;
            ForegroundOpacity = 1;
        }
    }


    /// <summary>
    /// The class describes the formating rules for the "message" classification type
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "suggestion.message")]
    [Name("suggestion.message")]
    [Order(Before = Priority.High, After = Priority.High)]

    internal sealed class SuggestionMessageFormat : ClassificationFormatDefinition
    {
        public SuggestionMessageFormat()
        {
            ForegroundColor = Colors.Goldenrod;
            IsBold = true;
            IsItalic = true;
            this.TextDecorations = null;
            this.TextEffects = null;
            BackgroundOpacity = 0;
            ForegroundOpacity = 1;
        }
    }
}
