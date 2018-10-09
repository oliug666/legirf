using OxyPlot;

namespace TPIH.Gecco.WPF.Core
{
    public static class PlotModelExtensions
    {
        /// <summary>
        /// Scale all axes so they show all data
        /// Refresh plot to notify UI
        /// </summary>
        /// <param name="plot"></param>
        
        public static void ResetAxesAndRefresh(this PlotModel plot)
        {
            foreach (var axis in plot.Axes)
            {
                axis.Reset();
            }
            plot.InvalidatePlot(true);
        }
    }
}
