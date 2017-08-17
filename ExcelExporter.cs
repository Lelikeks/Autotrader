using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace Autotrader
{
    public static class ExcelExporter
    {
        public static void Export<T>(IEnumerable<T> data, string fileName)
        {
            var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("data");

            var props = typeof(T).GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                ws.Cells[1, i + 1].Value = props[i].Name;
            }
            ws.Row(1).Style.Font.Bold = true;

            var num = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    ws.Cells[num, i + 1].Value = props[i].GetValue(item);
                }
                num++;
            }

            for (int i = 1; i <= props.Length; i++)
            {
                ws.Column(i).AutoFit();
            }

            pkg.SaveAs(new FileInfo(fileName));
        }
    }
}
