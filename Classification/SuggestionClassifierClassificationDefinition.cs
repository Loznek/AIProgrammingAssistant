using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace AIProgrammingAssistant.Classification
{
    /// <summary>
    /// Classification type definition export for SuggestionClassifier
    /// </summary>
    internal static class SuggestionClassifierClassificationDefinition
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        /// <summary>
        /// Defines the "SuggestionClassifier" classification types and subtypes.
        /// </summary>
        [Export]
        [Name("suggestion")]
        [BaseDefinition("CSharp")]
        internal static ContentTypeDefinition suggContentTypeDefinition = null;

        [Export]
        [FileExtension(".cs")]
        [ContentType("suggestion")]
        internal static FileExtensionToContentTypeDefinition csFileExtensionDefinition = null;

        [Export]
        [Name("suggestion")]
        internal static ClassificationTypeDefinition suggClassificationDefinition = null;

        [Export]
        [Name("suggestion.optimization")]
        [BaseDefinition("suggestion")]
        internal static ClassificationTypeDefinition suggOptimizationDefinition = null;

        [Export]
        [Name("suggestion.linq")]
        [BaseDefinition("suggestion")]
        internal static ClassificationTypeDefinition suggLinqDefinition = null;

        [Export]
        [Name("suggestion.message")]
        [BaseDefinition("suggestion")]
        internal static ClassificationTypeDefinition suggMessageDefinition = null;

#pragma warning restore 169
    }
}
