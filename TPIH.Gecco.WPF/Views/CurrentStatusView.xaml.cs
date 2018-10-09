using System;
using System.Collections.Generic;
using System.Linq;
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
using TPIH.Gecco.WPF.Helpers;

namespace TPIH.Gecco.WPF.Views
{
    /// <summary>
    /// Interaction logic for CurrentStatusView.xaml
    /// </summary>
    public partial class CurrentStatusView : UserControl
    {
        public CurrentStatusView()
        {
            int _height = 20;
            InitializeComponent();

            for (int i = 0; i < N3PR_Data.REG_NAMES.Count(); i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = N3PR_Data.REG_DESCRIPTION[i];
                tb.Height = _height;
                tb.VerticalAlignment = VerticalAlignment.Center;
                LabelsStackPanel.Children.Add(tb);
            }

            for (int i = 0; i < N3PR_Data.REG_NAMES.Count(); i++)
            {
                TextBox tb = new TextBox();
                // Value
                Binding bb = new Binding();
                bb.Path = new PropertyPath(string.Format("LatestValues[{0}]", i));
                BindingOperations.SetBinding(tb, TextBox.TextProperty, bb);
                // Enabled
                Binding bc = new Binding();
                bc.Path = new PropertyPath("LatestValuesEnabled");
                BindingOperations.SetBinding(tb, TextBox.IsEnabledProperty, bc);

                tb.Height = _height;
                tb.Width = 100;
                ValuesStackPanel.Children.Add(tb);
            }

            for (int i = 0; i < N3PR_Data.REG_NAMES.Count(); i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = N3PR_Data.REG_MEASUNIT[i];
                tb.Height = _height;
                tb.VerticalAlignment = VerticalAlignment.Center;
                UnitStackPanel.Children.Add(tb);
            }            
        }
    }
}
