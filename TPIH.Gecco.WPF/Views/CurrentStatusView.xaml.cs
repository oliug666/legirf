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
            int _titlesCnt = 0;
            InitializeComponent();

            // Check where to split
            List<int> idx = new List<int>();
            for (int ii = 0; ii < N3PR_Data.WHERE_SPLIT.Count; ii++)
            {
               if (ii != 0)
                    idx.Add(N3PR_Data.REG_NAMES.IndexOf(N3PR_Data.WHERE_SPLIT[ii])+1);
               else
                    idx.Add(N3PR_Data.REG_NAMES.IndexOf(N3PR_Data.WHERE_SPLIT[ii]));
            }
            idx.Add(-1);

            int i = 0;
            _titlesCnt = 0;
            while (i < N3PR_Data.REG_NAMES.Count())
            {
                if (i == idx[_titlesCnt])
                {
                    LabelsStackPanel.Children.Add(new TextBlock
                    {
                        Text = N3PR_Data.TITLES[_titlesCnt],
                        Height = _height,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontStyle = FontStyles.Normal,
                        FontWeight = FontWeights.Bold,
                        TextDecorations = TextDecorations.Underline,
                        FontSize = 16
                    });                    
                    if (_titlesCnt < idx.Count())
                        _titlesCnt++;
                }
                else
                {
                    LabelsStackPanel.Children.Add(new TextBlock
                    {
                        Text = N3PR_Data.REG_DESCRIPTION[i],
                        Height = _height,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    i++;
                }
            }

            i = 0; _titlesCnt = 0;
            while (i < N3PR_Data.REG_NAMES.Count())
            {                
                if (i == idx[_titlesCnt])
                {
                    ValuesStackPanel.Children.Add(new TextBlock
                    {
                        Text = "",
                        Height = _height,
                        FontSize = 16
                    });
                    if (_titlesCnt < idx.Count())
                        _titlesCnt++;
                }
                else
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
                    tb.IsReadOnly = true;
                    i++;
                    ValuesStackPanel.Children.Add(tb);
                }                
            }

            i = 0; _titlesCnt = 0;
            while (i < N3PR_Data.REG_NAMES.Count())
            {
                if (i == idx[_titlesCnt])
                {
                    UnitStackPanel.Children.Add(new TextBlock
                    {
                        Text = "",
                        Height = _height,
                        FontSize = 16
                    });
                    if (_titlesCnt < idx.Count())
                        _titlesCnt++;
                }
                else
                {
                    UnitStackPanel.Children.Add(new TextBlock
                    {
                        Text = N3PR_Data.REG_MEASUNIT[i],
                        Height = _height,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    i++;
                }
            }
        }
    }
}
