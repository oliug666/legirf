using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using TPIH.Gecco.WPF.Helpers;

namespace TPIH.Gecco.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            if (File.Exists("config_demo.csv"))
            {
                List<string> n3pr_Names = new List<string>();
                List<string> n3pr_Desc = new List<string>();
                List<string> n3pr_Unit = new List<string>();
                List<string> n3pr_Type = new List<string>();
                List<string> n3pr_DivFactor = new List<string>();
                List<string> n3pr_Present = new List<string>();
                using (var reader = new StreamReader(@"config_demo.csv", Encoding.GetEncoding("iso-8859-1")))
                {
                    // Skip first header line
                    reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');

                        // Name
                        n3pr_Names.Add(values[0]);
                        // Description
                        n3pr_Desc.Add(values[1]);
                        // Data type
                        n3pr_Unit.Add(values[2]);
                        if (values[3] == N3PR_Data.INT ||
                            values[3] == N3PR_Data.UINT ||
                            values[3] == N3PR_Data.BOOL)
                        {
                            n3pr_Type.Add(values[3]);
                        }
                        else
                        {
                            n3pr_Type.Add(N3PR_Data.INT);
                        }
                        // Div factor
                        if (Convert.ToDouble(values[4]) != 0)
                            n3pr_DivFactor.Add(values[4]);
                        else
                            n3pr_DivFactor.Add("1");
                        // Present
                        if (values[5] == "1" || values[5] == "0")
                            n3pr_Present.Add(values[5]);
                        else
                            n3pr_Present.Add("0");
                    }
                }

                if ((n3pr_Names.Count == n3pr_Desc.Count) &&
                    (n3pr_Names.Count == n3pr_Unit.Count) &&
                    (n3pr_Names.Count == n3pr_Type.Count) &&
                    (n3pr_Names.Count == n3pr_DivFactor.Count) &&
                    (n3pr_Names.Count == n3pr_Present.Count) && n3pr_Names.Count != 0)
                {
                    N3PR_Data.REG_NAMES.Clear();
                    N3PR_Data.REG_DESCRIPTION.Clear();
                    N3PR_Data.REG_MEASUNIT.Clear();
                    N3PR_Data.REG_TYPES.Clear();
                    N3PR_Data.REG_DIVFACTORS.Clear();
                    for (int i = 0; i < n3pr_Present.Count; i++)
                    {
                        if (n3pr_Present[i] == "1")
                        {
                            N3PR_Data.REG_NAMES.Add(n3pr_Names[i]);
                            N3PR_Data.REG_DESCRIPTION.Add(n3pr_Desc[i]);
                            N3PR_Data.REG_MEASUNIT.Add(n3pr_Unit[i]);
                            N3PR_Data.REG_TYPES.Add(n3pr_Type[i]);
                            N3PR_Data.REG_DIVFACTORS.Add(n3pr_DivFactor[i]);
                        }
                    }
                }
            }
        }
    }
}
