using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace daily.Formatters
{
    public class HtmlFormatter
    {
        static StringBuilder summarySB = new StringBuilder();

        public static async Task OutputHtmlFile(ThreeFund threeFund, int startYear, PerfCalculator perfCalc, int stock, int intl, int bond, Dictionary<string, FundValue> perfSummaries)
        {
            summarySB.Clear();
            var outputFile = new FileInfo($"{threeFund.StockFund.Symbol}{stock}-{intl}i.html");
            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            summarySB.AppendLine("<html>");
            summarySB.AppendLine("<head>");
            summarySB.AppendLine($"<title>{threeFund.StockFund.Symbol} {stock}/{bond} ({intl}i)</title>");
            summarySB.AppendLine($"<meta http-equiv='refresh' content='150' >");
            summarySB.AppendLine(
@"<style>
.right { text-align: right; font-size: 18pt;}
.rightSmall { text-align: right; font-size: 10pt}
.live {
  text-align:right;
  background:lightyellow;
  font-size:18pt;
  }
html, body{
    height: 100 %;
    width: 100 %;
    padding: 0;
    margin: 0;
  }
</style>");
            summarySB.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js@3.4.1/dist/chart.min.js'></script>");
            summarySB.AppendLine("</head>");
            summarySB.AppendLine("<body>");
            AppendDiv($"Performance for {stock}/{bond} ({intl}% intl)  {threeFund.StockFund.UpperSymbol}/{threeFund.BondFund.UpperSymbol} ({threeFund.InternationStockFund.UpperSymbol})");
            AppendDiv();

            double scale = 10000;
            double cummulativeValueYear = scale;
            double cummulativeValueMonth = double.NaN;
            string months = "['EOY','']";
            string days = "'EOM'";
            string monthValues = cummulativeValueYear.ToString("#.0");
            string dayValues = null;
            double lastDay = double.NaN;
            bool yearDone = false;
            int currentMonth = DateTime.Now.Month;
            foreach (var date in perfSummaries.Keys)
            {
                string[] chunks = date.Split('-');
                if (chunks.Length == 2 && !yearDone)
                {
                    cummulativeValueYear = cummulativeValueYear + perfSummaries[date].Value / 100 * scale;
                    string valueStr = cummulativeValueYear.ToString("##.00");
                    months += months == null ? $"'{chunks[1]}'" : $",['{chunks[1]}','{perfSummaries[date].Value.ToString("+##.00;-##.00")}%']";
                    monthValues += monthValues == null ? $"{valueStr}" : $",{valueStr}";
                    if (chunks[1] == DateTime.Now.AddMonths(-1).ToString("MMMM"))
                    {
                        cummulativeValueMonth = cummulativeValueYear;
                        dayValues = cummulativeValueMonth.ToString("#.0");
                    }
                }
                else if (chunks.Length == 3)
                {
                    cummulativeValueMonth = cummulativeValueMonth + perfSummaries[date].Value / 100 * scale;
                    string valueStr = cummulativeValueMonth.ToString("##.00");
                    string time = perfSummaries[date].Time == null ? null : $",'{perfSummaries[date].Time}'";
                    days += days == null ? $"'{chunks[2]}'" : $",['{currentMonth}/{int.Parse(chunks[2]).ToString()}','{perfSummaries[date].Value.ToString("+##.00;-##.00")}%'{time}]";
                    dayValues += dayValues == null ? $"{valueStr}" : $",{valueStr}";
                    lastDay = perfSummaries[date].Value;
                }
                else if (chunks.Length == 1)
                {
                    yearDone = true;
                }
            }

            summarySB.AppendLine(
        @"<script>
            function drawCharts() {
            var yearCtx = document.getElementById('yearChartCanvas').getContext('2d');
            var yearChart = new Chart(yearCtx, {
          type: 'line',
          height: 400,
          data:
            {
            labels:[" + months + @"],
            datasets:
                [{
                data:[" + monthValues + @"],
                label: '2021',
                borderColor: '#3e95cd',
                backgroundColor: '#7bb6dd',
                fill: false,
              }]
            },
          options:
            {
            responsive: true,
            maintainAspectRatio: true,
            scales: {
               x: {
                    ticks: {
                        font: {
                            size: 24,
                            },
                    }
                }
              }
            },
     });

     var monthCtx = document.getElementById('monthChartCanvas').getContext('2d');
            var monthChart = new Chart(monthCtx, {
          type: 'line',
          height: 400,
          data:
            {
            labels:[" + days + @"],
            datasets:
                [{
                data:[" + dayValues + @"],
                label: '" + DateTime.Now.ToString("MMMM") + @"',
                borderColor: '#3e95cd',
                backgroundColor: '#7bb6dd',
                fill: false,
              }]
            },
          options:
            {
            responsive: true,
            maintainAspectRatio: true,
                        scales: {
               x: {
                    ticks: {
                        font: {
                            size: 24,
                            },
                    }
                }
              }
            },
     });
     }
            window.onload = drawCharts;
            window.document.title = '" + lastDay.ToString("+##.00;-##.00") + @"%  ' + window.document.title; 
    </script>");
            AppendDiv();
            CreateHtmlPerfBody(perfSummaries);
            summarySB.AppendLine("</body><html>");
            await File.WriteAllTextAsync(outputFile.FullName, summarySB.ToString());
            Console.Write(".");
        }

        private static void AppendDiv(string line = "&nbsp", string className = null)
        {
            if (className == null)
            {
                summarySB.AppendLine($"<div>{line}</div>");
            }
            else
            {
                summarySB.AppendLine($"<div class={className}>{line}</div>");
            }
        }

        private static void AppendRow()
        {
            summarySB.AppendLine($"<tr><td>&nbsp;</td></tr>");
        }
        private static void StartTable()
        {
            summarySB.AppendLine($"<table style=width:100%;max-width:8in>");
        }
        private static void EndTable()
        {
            summarySB.AppendLine($"</table>");
        }
        private static void Append3Cells(string c0, string c1 = null, string c2 = null)
        {
            summarySB.AppendLine($"<tr><td class=right>{c0}</td><td class=right>{c1}</td><td class=right style=padding-right:7%>{c2}</td></tr>");
        }
        private static void Append3LiveCells(string c0, string c1 = null, string c2 = null)
        {
            summarySB.AppendLine($"<tr><td class=right>{c0}</td><td class=live>{c1}</td><td class=right style=padding-right:7%>{c2}</td></tr>");
        }
        private static void Append1CellRow(string c0)
        {
            summarySB.AppendLine($"<tr><td class=right colspan=3>{c0}</td></tr>");
        }


        private static void CreateHtmlPerfBody(Dictionary<string, FundValue> perfSummaries)
        {
            string year = null;
            string currentYear = DateTime.Now.Year.ToString();
            bool daysHeaderShown = false;
            StartTable();
            Append3Cells("", "Appreciation %", "Dividend %");

            foreach (var date in perfSummaries.Keys)
            {
                string[] chunks = date.Split('-');

                if (chunks[0] != year)
                {
                    year = chunks[0];
                    AppendRow();
                    if (daysHeaderShown)
                    {
                        Append3Cells($"{year}:");
                    }
                    else
                    {
                        Append1CellRow($"<canvas id=yearChartCanvas style=max-width:8in;max-height:6in></canvas>");
                    }
                }

                FundValue summaryData = perfSummaries[date];
                if (chunks.Length == 2)
                {
                    if (chunks[0] != currentYear)
                    {
                        Append3Cells(chunks[1], $"{summaryData.Value,7: ##.00;-##.00}%", $"{summaryData.Dividend:##.00}%");
                    }
                }
                else if (chunks.Length == 1)
                {
                    AppendRow();
                    string ytdStr = year == currentYear ? "YTD " : "Year";
                    if (chunks[0] == currentYear)
                    {
                        Append3LiveCells($"{chunks[0]} {ytdStr}", $"{summaryData.Value,6: ##.00;-##.00}%", $"{summaryData.Dividend,6: ##.00}%");
                    }
                    else
                    {
                        Append3Cells($"{chunks[0]} {ytdStr}", $"{summaryData.Value,6: ##.00;-##.00}%", $"{summaryData.Dividend,6: ##.00}%");
                    }
                }
                else
                {
                    if (!daysHeaderShown)
                    {
                        AppendRow();
                        Append1CellRow("<canvas id=monthChartCanvas style=max-width:8in;max-height:6in></canvas>");
                        daysHeaderShown = true;
                    }

                    string time = null;
                    if (summaryData.Time != null)
                    {
                        time = $"<br><span style=font-size:9pt>{summaryData.Time}</span>";
                    }
                }
            }

            AppendRow();
            EndTable();
        }
    }
}
