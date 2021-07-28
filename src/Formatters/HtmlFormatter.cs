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
            var outputFile = new FileInfo($"{threeFund.StockFund.Symbol}\\{stock}s-{bond}b-{intl}i.html");
            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            summarySB.AppendLine("<html>");
            summarySB.AppendLine("<head>");
            summarySB.AppendLine($"<title>{threeFund.StockFund.Symbol} {stock}/{bond} ({intl}i)</title>");
            summarySB.AppendLine($"<meta http-equiv='refresh' content='150' >");
            summarySB.AppendLine("<style> .right { text-align: right; font-size: 18pt} " +
            @"html, body{
                height: 100 %;
                    width: 100 %;
                    padding: 0;
                    margin: 0;
                    }" +
                        "</style>");
            summarySB.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js@3.4.1/dist/chart.min.js'></script>");
            summarySB.AppendLine("</head>");
            summarySB.AppendLine("<body>");
            AppendDiv($"Performance for {stock}/{bond} ({intl}% intl)  {threeFund.StockFund.UpperSymbol}/{threeFund.BondFund.UpperSymbol} ({threeFund.InternationStockFund.UpperSymbol})");
            AppendDiv();

            double scale = 10000;
            double cummulativeValueYear = scale;
            double cummulativeValueMonth = double.NaN;
            string months = "'EOY'";
            string days = "'EOM'";
            string monthValues = cummulativeValueYear.ToString("#.0");
            string dayValues = null;
            double lastDay = double.NaN;
            bool yearDone = false;
            foreach (var date in perfSummaries.Keys)
            {
                string[] chunks = date.Split('-');
                if (chunks.Length == 2 && !yearDone)
                {
                    cummulativeValueYear = cummulativeValueYear + perfSummaries[date].Value / 100 * scale;
                    string valueStr = cummulativeValueYear.ToString("##.00");
                    months += months == null ? $"'{chunks[1]}'" : $",'{chunks[1]}'";
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
                    days += days == null ? $"'{chunks[2]}'" : $",'{chunks[2]}'";
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
            maintainAspectRatio: false,
            },
     });

     var monthCtx = document.getElementById('monthChartCanvas').getContext('2d');
            var monthChart = new Chart(monthCtx, {
          type: 'line',
          
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
            maintainAspectRatio: false,
            },
     });
     }
            window.onload = drawCharts;
            window.document.title = window.document.title + ' " + lastDay.ToString("+##.00;-##.00") + @"%'; 

    </script>");
            AppendDiv();
            CreateHtmlPerfBody(perfSummaries);
            summarySB.AppendLine("</body><html>");
            await File.WriteAllTextAsync(outputFile.FullName, summarySB.ToString());
            Console.Write(".");
        }

        private static void AppendDiv(string line = "&nbsp")
        {
            summarySB.AppendLine($"<div>{line}</div>");
        }
        private static void AppendRow()
        {
            summarySB.AppendLine($"<tr><td>&nbsp;</td></tr>");
        }
        private static void StartTable()
        {
            summarySB.AppendLine($"<table style=width:100%>");
        }
        private static void EndTable()
        {
            summarySB.AppendLine($"</table>");
        }
        private static void Append3Cells(string c0, string c1 = null, string c2 = null)
        {
            summarySB.AppendLine($"<tr><td class=right>{c0}</td><td class=right>{c1}</td><td class=right>{c2}</td></tr>");
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
            var daysRow = new StringBuilder();

            foreach (var date in perfSummaries.Keys)
            {
                string[] chunks = date.Split('-');

                if (chunks.Length == 2 && daysRow.Length > 0)
                {
                    Append1CellRow($"<table style=width:100%;font-size:14pt class=right><td style=width:7%>% change</td>" + daysRow.ToString() + "</table>");
                    daysRow.Clear();
                    AppendRow();
                }
                
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
                        Append1CellRow($"<canvas id=yearChartCanvas></canvas>");
                    }
                }

                FundValue summaryData = perfSummaries[date];
                if (chunks.Length == 2)
                {
                    Append3Cells(chunks[1], $"{summaryData.Value,7: ##.00;-##.00}%", $"{summaryData.Dividend:##.00}%");
                }
                else if (chunks.Length == 1)
                {
                    AppendRow();
                    string ytdStr = year == currentYear ? "YTD " : "Year";
                    Append3Cells($"{chunks[0]} {ytdStr}", $"{summaryData.Value,6: ##.00;-##.00}%", $"{summaryData.Dividend,6: ##.00}%");
                }
                else
                {
                    if (!daysHeaderShown)
                    {
                        AppendRow();
                        Append1CellRow("<canvas id=monthChartCanvas></canvas>");
                        daysHeaderShown = true;
                    }

                    string time = null;
                    string fontSize = "10";
                    if (summaryData.Time != null)
                    {
                        time = $"<br><span style=font-size:9pt>{summaryData.Time}</span>";
                        fontSize = "14";
                    }

                    daysRow.Append($"<td style=font-size:{fontSize}pt>{summaryData.Value,6: ##.00;-##.00}%{time}</td>");
                }
            }

            AppendRow();
            EndTable();
        }
    }
}
