using OxyPlot;
using OxyPlot.Annotations;

namespace TPIH.Gecco.WPF.Helpers
{
    public class TooltipAnnotation : LineAnnotation
    {
        public string Tooltip { get; set; }
        public override void Render(IRenderContext rc, PlotModel model)
        {
            OxyPlot.Wpf.ShapesRenderContext src = rc as OxyPlot.Wpf.ShapesRenderContext;

            if (src != null)
                src.SetToolTip(Tooltip);

            base.Render(rc, model);            
        }
    }
}
