using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BulkIn.Desktop.Views
{
    public partial class LogsView : UserControl
    {
        private ScrollViewer? _scrollViewer;
        private bool _userScrolled;

        public LogsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _scrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
        }

        private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_scrollViewer == null || DataContext is not ViewModels.LogsViewModel viewModel)
                return;

            // Check if user manually scrolled (not at bottom)
            var isAtBottom = _scrollViewer.Offset.Y >= _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height - 10;
            
            if (!isAtBottom && e.OffsetDelta.Y < 0)
            {
                // User scrolled up manually
                _userScrolled = true;
                viewModel.AutoScroll = false;
            }
            else if (isAtBottom && _userScrolled)
            {
                // User scrolled back to bottom after manual scroll
                _userScrolled = false;
                if (!viewModel.AutoScroll)
                {
                    viewModel.AutoScroll = true;
                }
            }
        }

        public void ScrollToBottom()
        {
            _scrollViewer?.ScrollToEnd();
        }
    }
}
