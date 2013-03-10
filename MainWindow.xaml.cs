using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.IO;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Win32;

namespace Inverse.XPath
{
    public partial class MainWindow
    {
        private static readonly Brush TextBrush = new SolidColorBrush(Colors.Black);
        private static readonly Brush ErrorBrush = new SolidColorBrush(Colors.Red);

        private Stream annotationStream;
        private AnnotationService service;
        private string document;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            service = AnnotationService.GetService(docReader);

            if (service == null)
            {
                annotationStream = new MemoryStream();
                service = new AnnotationService(docReader);
                var store = new XmlStreamStore(annotationStream);

                service.Enable(store);                
            }

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                handler => this.expressionTextBox.TextChanged += handler,
                handler => this.expressionTextBox.TextChanged -= handler)
                .Throttle(TimeSpan.FromMilliseconds(800))
                .ObserveOnDispatcher()
                .Select(args => this.expressionTextBox.Text)
                .DistinctUntilChanged()
                .Where(text => !String.IsNullOrWhiteSpace(text))
                .Subscribe(this.FindMatches);
        }

        private void WindowUnloaded(object sender, RoutedEventArgs e)
        {
            if (service != null && service.IsEnabled)
            {
                // Flush annotations to stream.
                service.Store.Flush();

                // Disable annotations.
                service.Disable();
                annotationStream.Close();
            }
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            var myDialog = new OpenFileDialog
            {
                Filter = "Web Pages(*.HTML;*.HTM)|*.HTML;*.HTM|All files (*.*)|*.*",
                CheckFileExists = true
            };

            if (myDialog.ShowDialog() == true)
            {
                this.document = File.ReadAllText(myDialog.FileName);
                this.LoadDocument();
            }
        }

        private void ClipboardButtonClick(object sender, RoutedEventArgs e)
        {
            this.document = Clipboard.GetText();
            this.LoadDocument();
        }

        private void MatchButtonClick(object sender, RoutedEventArgs e)
        {
            var annotations = new StringBuilder();

            foreach (var annotation in this.service.Store.GetAnnotations())
            {
                var annotationText = GetAnnotationText(annotation);

                if (annotationText != null)
                {
                    annotations.AppendLine(annotationText);
                }
            }

            if (annotations.Length > 0)
            {
                this.document = annotations.ToString();
                this.LoadDocument();
            }
        }

        private void WordWrapCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            this.SetWordWrap();
        }

        private void LoadDocument()
        {
            this.docReader.CreateDocumentFromText(this.document);
            this.SetWordWrap();
            this.FindMatches(this.expressionTextBox.Text);
        }

        private void FindMatches(string xpath)
        {
            // Clear any previous errors
            this.ClearExpressionError();

            this.service.ClearMatches();

            if (String.IsNullOrEmpty(this.document))
            {
                return;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(this.document);

            try
            {
                var timer = Stopwatch.StartNew();
                var matches = htmlDocument.DocumentNode.SelectNodes(xpath);
                var duration = timer.ElapsedMilliseconds;

                if (matches == null)
                {
                    this.countTextBlock.Text = MessageStrings.NoMatchesFoundText;
                    return;
                }

                foreach (var match in matches)
                {
                    var html = match.OuterHtml;

                    // Cannot annotate an empty selection (this should never occur anyway)
                    if (html.Length < 1)
                    {
                        continue;
                    }

                    this.docReader.HighlightText(this.service, match.StreamPosition, html.Length);
                }

                this.countTextBlock.Text = String.Format(MessageStrings.MatchesFoundText, matches.Count, duration);
            }
            catch (XPathException e)
            {
                this.countTextBlock.Text = MessageStrings.NoMatchesFoundText;
                this.SetExpressionError(e.Message);
            }
        }

        private void SetExpressionError(string message)
        {
            this.expressionTextBox.Foreground = MainWindow.ErrorBrush;
            this.expressionTextBox.ToolTip = message;
        }

        private void ClearExpressionError()
        {
            this.expressionTextBox.Foreground = MainWindow.TextBrush;
            this.expressionTextBox.ToolTip = null;
        }

        private void SetWordWrap()
        {
            if (String.IsNullOrEmpty(this.document))
            {
                return;
            }

            var wordWrap = this.wordWrapToggleButton.IsChecked;

            if (wordWrap == true)
            {
                this.docReader.Document.PageWidth = Double.NaN;
            }
            else
            {
                this.docReader.SetDynamicPageWidth(this.document);
            }
        }

        private string GetAnnotationText(Annotation annotation)
        {
            var anchorInfo = AnnotationHelper.GetAnchorInfo(service, annotation);
            var resolvedAnchor = anchorInfo.ResolvedAnchor as TextAnchor;

            if (resolvedAnchor != null)
            {
                var startPointer = (TextPointer)resolvedAnchor.BoundingStart;
                var endPointer = (TextPointer)resolvedAnchor.BoundingEnd;
                var range = new TextRange(startPointer, endPointer);

                return range.Text;
            }

            return null;
        }
    }
}