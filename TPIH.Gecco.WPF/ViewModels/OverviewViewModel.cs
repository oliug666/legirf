using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Helpers;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class OverviewViewModel : MultiPlotViewModel
    {                
        public OverviewViewModel(List<string> PlotNames)
        {
            if (PlotNames == null)
                return;
            if (PlotNames.Count != 4)
                return;

            // If everything is ok, then go
            XDocument doc = new XDocument();
            try
            {
                doc = XDocument.Load("config.xml");
                IsFileLoaded = Visibility.Visible;
            }
            catch
            {
                //GlobalCommands.ShowError.Execute(new Exception("Impossible to load XML configuration file."));
                IsFileLoaded = Visibility.Collapsed;
                return;
            }

            var p00 = doc.Root.Descendants(PlotNames[0]);            
            RegNames00 = Parser.ParseXmlElement(p00.Elements("reg_name").Nodes());
            RegDescriptions00 = GetRegDescription(RegNames00);
            RegUnits00 = GetRegUnits(RegNames00);

            var p01 = doc.Root.Descendants(PlotNames[1]);
            RegNames01 = Parser.ParseXmlElement(p01.Elements("reg_name").Nodes());
            RegDescriptions01 = GetRegDescription(RegNames01);
            RegUnits01 = GetRegUnits(RegNames01);

            var p10 = doc.Root.Descendants(PlotNames[2]);
            RegNames10 = Parser.ParseXmlElement(p10.Elements("reg_name").Nodes());
            RegDescriptions10 = GetRegDescription(RegNames10);
            RegUnits10 = GetRegUnits(RegNames10);

            var p11 = doc.Root.Descendants(PlotNames[3]);
            RegNames11 = Parser.ParseXmlElement(p11.Elements("reg_name").Nodes());
            RegDescriptions11 = GetRegDescription(RegNames11);
            RegUnits11 = GetRegUnits(RegNames11);

            // Create Plots
            if (RegDescriptions00 == null && RegDescriptions01 == null
                && RegDescriptions10 == null && RegDescriptions11 == null)
            {
                IsFileLoaded = Visibility.Collapsed;
                return;
            }
            else
            {
                Plot00 = CreatePlotModel(RegDescriptions00, RegUnits00);
                Plot01 = CreatePlotModel(RegDescriptions01, RegUnits01);
                Plot10 = CreatePlotModel(RegDescriptions10, RegUnits10);
                Plot11 = CreatePlotModel(RegDescriptions11, RegUnits11);
                // Subscribe to event(s)
                EventAggregator.OnAlarmMessageTransmitted += OnFlaggedAlarmMessageReceived;
                DriverContainer.Driver.OnDataRetrievalCompleted += new EventHandler(DataRetrievedEventHandler);
                DriverContainer.Driver.OnConnectionStatusChanged += new EventHandler(ConnectionStatusChangedEventHandler);
            }
        }

        private void ConnectionStatusChangedEventHandler(object sender, EventArgs e)
        {
            if (!DriverContainer.Driver.IsConnected)
            {
                ClearAll(Plot00);
                ClearAll(Plot01);
                ClearAll(Plot10);
                ClearAll(Plot11);
            }
        }

        private void DataRetrievedEventHandler(object sender, EventArgs e)
        {
            if (IsFileLoaded == Visibility.Visible)
            {
                AddSeries(Plot00, RegNames00);
                AddSeries(Plot01, RegNames01);
                AddSeries(Plot10, RegNames10);
                AddSeries(Plot11, RegNames11);
            }
        }        
    }
}
