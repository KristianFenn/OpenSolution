using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace OpenSolution
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Solution> SolutionList { get; set; }
        public Solution Selected { get; set; }
        public string Filter { get; set; }
        public bool EnableControls { get; set; }

        private NotifyIcon _notifyIcon;
        private GlobalHotkeyHandler _openHotkey;

        public MainWindow()
        {
            SolutionList = new ObservableCollection<Solution>();
            Selected = SolutionList.FirstOrDefault();
            EnableControls = false;
            InitializeComponent();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = Properties.Resources.Open;
            _notifyIcon.Visible = false;
            _notifyIcon.Click += (sender, args) =>
            {
                ShowWindow();
            };

            ReloadSolutions();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            _openHotkey = new GlobalHotkeyHandler(this,
                GlobalHotkeyHandler.Modifiers.MOD_CTRL | GlobalHotkeyHandler.Modifiers.MOD_ALT | GlobalHotkeyHandler.Modifiers.MOD_NOREPEAT,
                GlobalHotkeyHandler.VirtualKey.O);

            _openHotkey.OnHotkeyPressed += (sender, args) =>
            {
                ShowWindow();
            };

            base.OnSourceInitialized(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _openHotkey.Dispose();
            base.OnClosed(e);
        }

        private void ShowWindow()
        {
            ResetFilter();
            Show();
            WindowState = WindowState.Normal;
            _notifyIcon.Visible = false;
            Activate();
        }

        private void ResetFilter()
        {
            Filter = "";
            NotifyPropertyChanged(w => w.Filter);
            filterText.Focus();
        }

        private void HideWindow()
        {
            Hide();
            _notifyIcon.Visible = true;
        }

        private void FilterChanged(object sender, TextChangedEventArgs e)
        {
            SolutionList.Clear();

            foreach (var solution in Solutions.Get(Filter))
            {
                SolutionList.Add(solution);
            }

            Selected = SolutionList.FirstOrDefault();
            NotifyPropertyChanged(x => x.Selected);
        }

        private void ReloadClicked(object sender, RoutedEventArgs e)
        {
            ReloadSolutions();
        }

        private void ReloadSolutions()
        {
            EnableControls = false;
            NotifyPropertyChanged(x => x.EnableControls);

            SolutionList.Clear();
            Task.Run(() => Solutions.LoadSolutions(AppSettings.SolutionPath))
                .ContinueWith(task =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var solution in Solutions.Get())
                        {
                            SolutionList.Add(solution);
                        }
                        EnableControls = true;
                        NotifyPropertyChanged(x => x.EnableControls);
                        filterText.Focus();
                    });
                });
        }

        private void SolutionDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (Selected == null)
                return;

            Process.Start(Selected.Path);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        if (Selected == null)
                        {
                            return;
                        }
                        Process.Start(Selected.Path);
                        HideWindow();
                        break;
                    }
                case Key.Down:
                    {
                        if (Selected == null)
                        {
                            return;
                        }
                        var index = SolutionList.IndexOf(Selected);
                        index++;

                        if (index < SolutionList.Count)
                        {
                            Selected = SolutionList[index];
                            NotifyPropertyChanged(x => x.Selected);
                        }

                        break;
                    }
                case Key.Up:
                    {
                        if (Selected == null)
                        {
                            return;
                        }
                        var index = SolutionList.IndexOf(Selected);
                        index--;

                        if (index >= 0)
                        {

                            Selected = SolutionList[index];
                            NotifyPropertyChanged(x => x.Selected);
                        }

                        break;
                    }
                case Key.Escape:
                    {
                       if (!string.IsNullOrEmpty(Filter))
                        {
                            ResetFilter();
                        }
                        else
                        {
                            HideWindow();
                        }
                        break;
                    }
                case Key.F5:
                    {
                        ReloadSolutions();
                        break;
                    }
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                HideWindow();
            }

            base.OnStateChanged(e);
        }

        private void NotifyPropertyChanged<T>(Expression<Func<MainWindow, T>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }
    }
}
