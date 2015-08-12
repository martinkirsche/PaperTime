using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MarkdownDeep;
using System.Text;
using PaperTime.Data;

namespace PaperTime
{
    public class Renderer
    {
        static string template = LoadFromResource("PaperTime.Template.Body.html");
        static string styles = LoadFromResource("PaperTime.Template.Styles.css");

        static Markdown markdown = new Markdown()
        {
            ExtraMode = true
        };

        static string LoadFromResource(string name)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                return new StreamReader(s).ReadToEnd();
            }            
        }

        public static List<string> Save(Report report, Format format)
        {
            var file = new FileInfo(report.FileName);
            if (!file.Directory.Exists) { file.Directory.Create(); }
            List<string> resultingFileNames = new List<string>();
            using (var ms = new MemoryStream())
            {
                TextWriter output = new StreamWriter(ms);
                string headLine;
                Render(report, output, out headLine);
                output.Flush();
                ms.Position = 0;
                string content = new StreamReader(ms).ReadToEnd();

                if ((format & Format.Markdown) != 0)
                {
                    using (var w = new StreamWriter(file.FullName))
                    {
                        w.Write(content);
                    }
                    resultingFileNames.Add(file.FullName);
                }
                if ((format & Format.Html) != 0)
                {
                    var fileName = Path.Combine(
                        Path.GetDirectoryName(file.FullName),
                        Path.GetFileNameWithoutExtension(file.FullName)) + ".html";
                    using (var w = new StreamWriter(fileName))
                    {
                        w.Write(template, headLine, markdown.Transform(content), styles);
                    }
                    resultingFileNames.Add(fileName);
                }
            }
            return resultingFileNames;
        }

        public static string Preview(Report report)
        {
            var output = new StringBuilder();
            foreach (KeyValuePair<string, TimeSpan> category in report.Categories)
            {
                if (!Report.CategoryIsRoot(category.Key)) { continue; }
                output.AppendLine(string.Format(
                    "{1:0.00} h ({2:0.0 %} / {3:0.0 %}) @ {0}",
                    category.Key,
                    category.Value.TotalHours,
                    (category.Value.TotalSeconds / report.TimeActual.TotalSeconds),
                    (report.CategoriesOverall[category.Key].TotalHours / report.TimeOverall.TotalHours)));
            }
            output.AppendLine(string.Format("= {0:0.00} h", report.TimeSaldo.TotalHours));
            return output.ToString();
        }

        public static void Render(Report report, TextWriter output, out string headLine)
        {
            if (report.FirstDay.Year == report.LastDay.Year && report.FirstDay.Month == report.LastDay.Month)
            {
                headLine = string.Format(Text.ReportSingleMonthHeading, report.FirstDay);
            }
            else
            {
                headLine = string.Format(Text.ReportDatePeriodHeading, report.FirstDay, report.LastDay);
            }
            HeadingLevel1Write(output, headLine);
            output.WriteLine();
            DaysWrite(output, report);
            output.WriteLine();
            TimeWrite(output, report);
            output.WriteLine();
            CategoriesWrite(output, report);
            output.WriteLine();
            RecordsWrite(output, report);
        }

        static void HeadingLevel1Write(TextWriter output, string heading)
        {
            output.WriteLine(heading);
            output.WriteLine(new string('=', heading.Length));
            output.WriteLine();
        }

        static void HeadingLevel2Write(TextWriter output, string heading)
        {
            output.WriteLine(heading);
            output.WriteLine(new string('-', heading.Length));
            output.WriteLine();
        }

        static void HeadingLevel3Write(TextWriter output, string heading)
        {
            output.Write("### ");
            output.WriteLine(heading);
            output.WriteLine();
        }

        static void DaysWrite(TextWriter output, Report report)
        {
            HeadingLevel2Write(output, Text.ReportWorkdaysHeading);
            output.WriteLine(Text.ReportWorkdaysDescription1,
                string.Format(
                    report.FirstDay.Year == report.LastDay.Year &&
                    report.FirstDay.Month == report.LastDay.Month &&
                    report.FirstDay.Day == 1 &&
                    report.LastDay.Day == DateTime.DaysInMonth(report.LastDay.Year, report.LastDay.Month) ?
                        Text.ReportWorkdaysDescription2 : Text.ReportWorkdaysDescription3, report.FirstDay, report.LastDay),
                        report.Days);
            output.WriteLine();
            if (report.Holidays.Count > 0)
            {
                HeadingLevel3Write(output, Text.ReportHolidaysHeading);
                foreach (var h in report.Holidays)
                {
                    output.WriteLine(Text.ReportHolidaysEntry, h.Date, h.Name);
                    output.WriteLine();
                }
            }
        }

        static void TimeWrite(TextWriter output, Report report)
        {
            HeadingLevel2Write(output, Text.ReportTargetActualHeading);
            output.WriteLine(Text.ReportTargetTimeEntry,
                report.Days,
                report.TimeTarget.TotalHours / report.Days,
                report.TimeTarget.TotalHours);
            output.WriteLine();
            output.WriteLine(Text.ReportActualTimeEntry, report.TimeActual.TotalHours);
            output.WriteLine();
            output.WriteLine(Text.ReportSaldoTimeEntry,
                report.TimePreviousSaldo.TotalHours,
                report.TimeActual.TotalHours,
                report.TimeTarget.TotalHours,
                report.TimeSaldo.TotalHours);
        }

        static void CategoriesWrite(TextWriter output, Report report)
        {
            HeadingLevel2Write(output, Text.ReportCategoryHeading);
            TableWrite(output,
                new string[] 
                {
                    Text.ReportCategoryColumn1, 
                    Text.ReportCategoryColumn2, 
                    Text.ReportCategoryColumn3, 
                    Text.ReportCategoryColumn4 
                },
                CategoriesToTable(report));
        }

        static IEnumerable<IEnumerable<string>> CategoriesToTable(Report report)
        {
            foreach (KeyValuePair<string, TimeSpan> category in report.Categories)
            {
                yield return
                    new string[] 
                    { 
                        (Report.CategoryIsRoot(category.Key) ?
                            ("**"+category.Key+"**") : category.Key), 
                        category.Value.TotalHours.ToString("0.00 h"), 
                        (category.Value.TotalHours / report.TimeActual.TotalHours).ToString("0.0 %"),
                        (report.CategoriesOverall[category.Key].TotalHours / report.TimeOverall.TotalHours).ToString("0.0 %")
                    };
            }
        }

        static void RecordsWrite(TextWriter output, Report report)
        {
            HeadingLevel2Write(output, Text.ReportRecordHeading);
            TableWrite(output,
                new string[] { 
                    Text.ReportRecordColumn1,
                    Text.ReportRecordColumn2,
                    Text.ReportRecordColumn3,
                    Text.ReportRecordColumn4,
                    Text.ReportRecordColumn5
                },
                RecordsToTable(report));
        }

        static IEnumerable<IEnumerable<string>> RecordsToTable(Report report)
        {
            foreach (Record r in report.Records)
            {
                yield return
                    new string[] 
                    { 
                        r.Begin.Date.ToShortDateString(), 
                        r.Begin.ToShortTimeString(),
                        r.End.ToShortTimeString(), 
                        r.Duration.TotalHours.ToString("0.00 h"), 
                        r.Category 
                    };
            }
        }

        static void TableWrite(TextWriter output, string[] tableHeading, IEnumerable<IEnumerable<string>> data)
        {
            var th = tableHeading.ToArray();
            output.WriteLine(string.Join(" | ", th).TrimEnd());
            output.WriteLine(string.Join("-|-", th.Select(h => new string('-', h.Length)).ToArray()));
            foreach (var row in data)
            {
                output.WriteLine(string.Join(" | ",
                    th.Zip(row, (h, c) =>
                        0 == c.Length || !(c[0] >= '0' && c[0] <= '9') ?
                            c.PadRight(h.Length) :
                            c.PadLeft(h.Length)).ToArray()).TrimEnd());
            }
        }
    }

    public static class ZipExtension
    {
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");

            using (var iteratorFirst = first.GetEnumerator())
            {
                using (var iteratorSecond = second.GetEnumerator())
                {
                    while (iteratorFirst.MoveNext() && iteratorSecond.MoveNext())
                    {
                        yield return resultSelector(iteratorFirst.Current, iteratorSecond.Current);
                    }
                }
            }
        }
    }
}
