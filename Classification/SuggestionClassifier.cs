using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;

namespace AIProgrammingAssistant.Classification
{
    /// <summary>
    /// Classifier that classifies all text as an instance of the "SuggestionClassifier" classification type.
    /// The solution of the classification based on the following sample project: Microsoft: Diff Classifier Sample, https://github.com/microsoft/VSSDK-Extensibility-Samples/tree/master/Diff_Classifier
    /// </summary>
    internal class SuggestionClassifier : IClassifier
    {
        /// <summary>
        /// Classification type.
        /// </summary>
        IClassificationTypeRegistryService _classificationTypeRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestionClassifier"/> class.
        /// </summary>
        internal SuggestionClassifier(IClassificationTypeRegistryService registry)
        {
            this._classificationTypeRegistry = registry;
        }

        /// <summary>
        /// Classify the given spans, which, for diff files, classifies a line at a time.
        /// </summary>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            if (snapshot.Length == 0)
                return spans;

            int startNumber = span.Start.GetContainingLine().LineNumber;
            int endNumber = (span.End - 1).GetContainingLine().LineNumber;

            for (int i = startNumber; i <= endNumber; i++)
            {
                ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);

                IClassificationType type = null;

                string text = line.Snapshot.GetText(new SnapshotSpan(line.Start, line.End));

                text = text.TrimStart();
                if (text.StartsWith(SuggestionLineSign.optimization, StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("suggestion.optimization");
                else if (text.StartsWith(SuggestionLineSign.linq, StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("suggestion.linq");
                else if (text.StartsWith(SuggestionLineSign.message, StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("suggestion.message");

                if (type != null)
                    spans.Add(new ClassificationSpan(line.Extent, type));
            }

            return spans;
        }

#pragma warning disable 67

    /// <summary>
    /// An event that occurs when the classification of a span of text has changed.
    /// </summary>
    public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

    }
}
