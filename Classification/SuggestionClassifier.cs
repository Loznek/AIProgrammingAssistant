using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;

namespace AIProgrammingAssistant.Classification
{
    /// <summary>
    /// Classifier that classifies all text as an instance of the "SuggestionClassifier" classification type.
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
        /// <param name="registry">Classification registry.</param>
        internal SuggestionClassifier(IClassificationTypeRegistryService registry)
        {
            this._classificationTypeRegistry = registry;
        }
        #region Public Methods
        /// <summary>
        /// Classify the given spans, which, for diff files, classifies
        /// a line at a time.
        /// </summary>
        /// <param name="span">The span of interest in this projection buffer.</param>
        /// <returns>The list of <see cref="ClassificationSpan"/> as contributed by the source buffers.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            if (snapshot.Length == 0)
                return spans;

            int startno = span.Start.GetContainingLine().LineNumber;
            int endno = (span.End - 1).GetContainingLine().LineNumber;

            for (int i = startno; i <= endno; i++)
            {
                ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);

                IClassificationType type = null;

                string text = line.Snapshot.GetText(
                        new SnapshotSpan(line.Start, line.End));
                text = text.TrimStart();
                if (text.StartsWith(SuggestionLineSign.optimization, StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("suggestion.optimization");
                else if (text.StartsWith(SuggestionLineSign.linq, StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("suggestion.linq");
                else if (text.StartsWith(SuggestionLineSign.optimization, StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("suggestion.message");

                if (type != null)
                    spans.Add(new ClassificationSpan(line.Extent, type));
            }

            return spans;
        }



        #endregion // Public Methods
    #region IClassifier

#pragma warning disable 67

    /// <summary>
    /// An event that occurs when the classification of a span of text has changed.
    /// </summary>
    /// <remarks>
    /// This event gets raised if a non-text change would affect the classification in some way,
    /// for example typing /* would cause the classification to change in C# without directly
    /// affecting the span.
    /// </remarks>
    public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

        /// <summary>
        /// Gets all the <see cref="ClassificationSpan"/> objects that intersect with the given range of text.
        /// </summary>
        /// <remarks>
        /// This method scans the given SnapshotSpan for potential matches for this classification.
        /// In this instance, it classifies everything and returns each span as a new ClassificationSpan.
        /// </remarks>
        /// <param name="span">The span currently being classified.</param>
        /// <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
        

        #endregion
    }
}
