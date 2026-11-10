using System;
using System.IO;
using System.Text;
using MultiScanX.Core.Models;

namespace MultiScanX.Core.Services
{
    /// <summary>
    /// 계측 결과 데이터 파일(.txt / .csv) 자동 저장 서비스
    /// </summary>
    public class DataLoggerService
    {
        public string ExportToTextFile(SubstrateScanResult res, string outputDir)
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string timestamp = res.StartTime.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{res.LotId}_{res.SubstrateId}_{timestamp}.txt";
            string fullPath = Path.Combine(outputDir, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("# ==============================================================================");
            sb.AppendLine("# KCT MultiSensor-Metrology Inspection Data Report");
            sb.AppendLine("# ==============================================================================");
            sb.AppendLine($"# Lot ID          : {res.LotId}");
            sb.AppendLine($"# Substrate ID    : {res.SubstrateId}");
            sb.AppendLine($"# Recipe Name     : {res.RecipeName}");
            sb.AppendLine($"# Start Time      : {res.StartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# End Time        : {res.EndTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Total CycleTime : {res.TotalCycleTime.TotalSeconds:F1} sec");
            sb.AppendLine("# ==============================================================================");
            sb.AppendLine("Pt_ID\tTarget_X\tTarget_Y\tActual_X\tActual_Y\tScale_Z\tCu_Thick(um)\tFilm_Thick(nm)\tGOF\tLoad(g)\tJudgement");

            foreach (var p in res.PointResults)
            {
                sb.AppendLine($"{p.PointId:D2}\t{p.TargetX:F3}\t{p.TargetY:F3}\t{p.ActualX:F3}\t{p.ActualY:F3}\t{p.ActualZ:F4}\t" +
                              $"{p.CuThicknessUm:F3}\t{p.FilmThicknessNm:F1}\t{p.FilmGoodnessOfFit:F3}\t{p.ContactLoadG:F1}\t{(p.IsPass ? "PASS" : "FAIL")}");
            }

            sb.AppendLine("# ==============================================================================");
            sb.AppendLine("# Summary Statistics");
            sb.AppendLine($"# Cu Thickness (um) : Avg={res.CuAvgUm:F3}, Min={res.CuMinUm:F3}, Max={res.CuMaxUm:F3}");
            sb.AppendLine($"# Film Thickness(nm): Avg={res.FilmAvgNm:F1}");
            sb.AppendLine($"# Overall Result    : {(res.OverallJudgement ? "ALL PASS" : "FAIL")}");
            sb.AppendLine("# ==============================================================================");

            File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
            return fullPath;
        }
    }
}

