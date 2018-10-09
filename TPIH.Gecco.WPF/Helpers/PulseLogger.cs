using System.IO;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.Helpers
{
    public class PulseLogger
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public string CompletePath { get { return System.IO.Path.Combine(Path, FileName); } }
        public void Write(DataSummary data)
        {
            if (data == null) return;
            var printHeader = !File.Exists(CompletePath);
            var writer = new StreamWriter(CompletePath,true);
            if (printHeader)
            {
                writer.WriteLine("Time\tEstimatedDuration\tAvgBVal\tAvgIVal\tAvgUIVal");
            }
            writer.WriteLine(
                data.Time.ToString("yyyy-MM-dd HH:mm:ss")+"\t" +
                data.EstimatedDuration + "\t" +
                data.AvgB_val + "\t" +
                data.AvgI_val + "\t" +
                data.AvgUI_val + "\t" +
                "");
            writer.Close();
        }
    }
}
