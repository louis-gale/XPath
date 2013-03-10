using System;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Inverse.XPath
{
    internal static class FlowDocumentExtensions
    {
        private static readonly Random random = new Random();

        internal static void CreateDocumentFromText(this FlowDocumentScrollViewer viewer, string text)
        {
            var paragraph = new Paragraph
            {
                TextAlignment = TextAlignment.Left
            };

            paragraph.Inlines.Add(text);

            viewer.Document = new FlowDocument(paragraph);
        }

        internal static void SetDynamicPageWidth(this FlowDocumentScrollViewer viewer, string text)
        {
            // Use a TextBlock to measure and find the width of the target document text.  This is inefficient but the only way i can find without custom controls
            var textBlock = new TextBlock
            {
                FontFamily = viewer.FontFamily,
                FontSize = viewer.FontSize,
                Text = text
            };

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            viewer.Document.PageWidth = textBlock.DesiredSize.Width + 50;
        }

        internal static void HighlightText(this FlowDocumentScrollViewer viewer, AnnotationService service, int offset, int length)
        {
            var brush = new SolidColorBrush(Color.FromArgb(127, (byte)random.Next(200), (byte)random.Next(200), (byte)random.Next(200)));
            var startPos = viewer.Document.ContentStart.GetOffsetTextPointer(offset);
            var endPos = startPos.GetOffsetTextPointer(length);

            viewer.Selection.Select(startPos, endPos);

            AnnotationHelper.CreateHighlightForSelection(service, String.Empty, brush);
            //textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }
    }
}