using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TPIH.Gecco.WPF.Helpers;
using TPIH.Gecco.WPF.ViewModels;

namespace TPIH.Gecco.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _mainViewModel = new MainWindowViewModel();
            this.DataContext = _mainViewModel;
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
            e.Handled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(SharedResourceDictionary.SharedDictionary["D_ClosingWindow"] + "",
                SharedResourceDictionary.SharedDictionary["D_H_Closing"] + "", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                // Signal write to view-model
                _mainViewModel.TerminateExecutionAndCloseConnections();
                // Close the window
                Application.Current.Shutdown();
            }
            else
            {
                // Keep alive
                e.Cancel = true;
            }
        }
    }
}
