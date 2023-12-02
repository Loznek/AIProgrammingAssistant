using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace AIProgrammingAssistant.Classification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "suggestion.optimization")]
    [Name("suggestion.optimization")]
    [Order(Before = Priority.High, After = Priority.High)]
    internal sealed class SuggestionOptimizationFormat : ClassificationFormatDefinition
    {
        public SuggestionOptimizationFormat()
        {
            ForegroundColor = Colors.LightCoral;
            IsItalic = true;
            this.TextDecorations = null;
            this.TextEffects = null;
            BackgroundOpacity = 0.2;
            ForegroundOpacity = 0.8;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "suggestion.linq")]
    [Name("suggestion.linq")]
    [Order(Before = Priority.High, After = Priority.High)]
    internal sealed class SuggestionLinqFormat : ClassificationFormatDefinition
    {
        public SuggestionLinqFormat()
        {
            ForegroundColor = Colors.SandyBrown;
            IsBold = true;
            BackgroundOpacity = 0.8;
            this.TextDecorations = null;
            this.TextEffects = null;
            ForegroundOpacity = 0.8;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "suggestion.message")]
    [Name("suggestion.message")]
    [Order(Before = Priority.High, After = Priority.High)]

    internal sealed class SuggestionMessageMessageFormat : ClassificationFormatDefinition
    {
        public SuggestionMessageMessageFormat()
        {
            ForegroundColor = Colors.Orange;
            IsBold = true;
            IsItalic = true;
            this.TextDecorations = null;
            this.TextEffects = null;
            BackgroundOpacity = 0.8;
            ForegroundOpacity = 0.8;
        }
    }
}
