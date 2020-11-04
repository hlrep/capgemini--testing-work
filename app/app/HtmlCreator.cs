using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace app
{
    class HtmlCreator // Provides workflow for creating html files
    {
        static public async void CreateHtml(FileParser.CollectedFileData dataHtml) // Async create html-file using parsed data info
        {
            StringBuilder htmlContentBuilder = new StringBuilder(); // Create builder to work with file struct
            string date = DateTime.Now.ToString("dd.mm.yyyy"); // Time of begining file creating
            /*
             * Form a new html-document
            */
                htmlContentBuilder.AppendLine("<!DOCTYPE html>");
                htmlContentBuilder.AppendLine("<html lang=\"ru\" dir=\"ltr\">");
                htmlContentBuilder.AppendLine("<head>");
                htmlContentBuilder.AppendLine(string.Format("<title>КЛиДЗ#{0}</title>", HttpUtility.HtmlEncode(FileParser.FileCreatedCounter + 1))); // Set in-browser file name
                htmlContentBuilder.AppendLine("</head>");
                htmlContentBuilder.AppendLine("<body>");
                htmlContentBuilder.AppendLine("<style>");
                htmlContentBuilder.AppendLine(
                    "* { margin: 0; padding: 0; }" +
                    "body { margin: 10px; }" +
                    ".section { height: 100%; width: calc(100% - 20px); margin-bottom: 60px; }" +
                    ".section__title { margin-bottom: 20px; font-style: italic; }" +
                    ".section__title::after { content: ''; height: 3px; width: 100%; background-color: black; display: block; }" +
                    ".section__attention-message { color: red; font-weight: normal; margin-bottom: 20px; }" +
                    ".section__table { width: 100%; border-collapse: collapse; text-align: center; }" +
                    ".section__table__line:first-child { background-color: aqua; }" +
                    ".section__table__line:first-child, .section__table__line.last-column { font-weight: bold; }" +
                    ".section__table__line > td { border: 1px solid black; }" +
                    ".attention { background-color: lightgreen; }"); // Set css styles
                htmlContentBuilder.AppendLine("</style>");
                htmlContentBuilder.AppendLine("<section class=\"section\">");
                htmlContentBuilder.Append("<h2 class=\"section__title\">Кредитный лимит на ");
                htmlContentBuilder.Append(HttpUtility.HtmlEncode(date)); // Add 1st date stamp into document
                htmlContentBuilder.AppendLine("</h2>");
                htmlContentBuilder.AppendLine("<table class=\"section__table\">");
                htmlContentBuilder.AppendLine("<tr class=\"section__table__line\">");
                htmlContentBuilder.AppendLine("<td>Код покупателя</td>");
                htmlContentBuilder.AppendLine("<td>Наименование</td>");
                htmlContentBuilder.AppendLine("<td>Общий кредитный лимит</td>");
                htmlContentBuilder.AppendLine("<td>Использованный кредитный лимит</td>");
                htmlContentBuilder.AppendLine("<td>Остаток кредитного лимита (-) / Превышение кредитного лимита (+)</td>");
                htmlContentBuilder.AppendLine("</tr>");
                htmlContentBuilder.AppendLine("<tr class=\"section__table__line\">");
                /*
                 * Create 1st table
                */
                    htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHtml.id)));
                    htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHtml.name)));
                    dataHtml.creditLimit = TransformNumber(dataHtml.creditLimit);
                    htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHtml.creditLimit)));
                    dataHtml.creditExposure = TransformNumber(dataHtml.creditExposure);
                    htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHtml.creditExposure)));
                    dataHtml.overBudget = TransformNumber(dataHtml.overBudget);
                    htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHtml.overBudget)));
                /*
                 * -----------------------------------------
                */
                htmlContentBuilder.AppendLine("</tr>");
                htmlContentBuilder.AppendLine("</table>");
                htmlContentBuilder.AppendLine("</section>");
                htmlContentBuilder.AppendLine("<section class=\"section\">");
                htmlContentBuilder.Append("<h2 class=\"section__title\">Дебиторская задолженность на ");
                htmlContentBuilder.Append(HttpUtility.HtmlEncode(date)); // Add 2nd date stamp into document
                htmlContentBuilder.AppendLine("</h2>");
                htmlContentBuilder.AppendLine("<h3 class=\"section__attention-message\">");
                htmlContentBuilder.AppendLine("<b>Важно!</b> При погашении задолженности Компания настоятельно рекомендует Покупателю указывать в каждом платёжном поручении сумму, равную сумме каждой выставленной отгрузки.");
                htmlContentBuilder.AppendLine("</h3>");
                htmlContentBuilder.AppendLine("<table class=\"section__table\">");
                htmlContentBuilder.AppendLine("<tr class=\"section__table__line\">");
                htmlContentBuilder.AppendLine("<td>Системный номер документа</td>");
                htmlContentBuilder.AppendLine("<td>Счёт фактура</td>");
                htmlContentBuilder.AppendLine("<td>Товарная накладная</td>");
                htmlContentBuilder.AppendLine("<td>Тип операции</td>");
                htmlContentBuilder.AppendLine("<td>Дата документа</td>");
                htmlContentBuilder.AppendLine("<td>Дата платежа</td>");
                htmlContentBuilder.AppendLine("<td>Дней просрочки</td>");
                htmlContentBuilder.AppendLine("<td>Сумма</td>");
                htmlContentBuilder.AppendLine("<td>Валюта</td>");
                htmlContentBuilder.AppendLine("<td>Комментарий</td>");
                htmlContentBuilder.AppendLine("</tr>");
                /*
                 * Create 2nd table
                */
                    // Top of the table
                    foreach (string[] row in dataHtml.rowLines) // Get every string array and place into table
                    {
                        htmlContentBuilder.AppendLine("<tr class=\"section__table__line\">"); // Start new row
                        row[6] = TransformNumber(row[6]);
                        row[7] = TransformNumber(row[7]);
                        for (int counterRow = 0; counterRow < row.Length; counterRow++) // For each element of new row
                        {
                            htmlContentBuilder.Append("<td"); // Open new element
                            if (counterRow == 6) // If element number equal 6
                            {
                                if (!Regex.IsMatch(row[6], @"-")) htmlContentBuilder.Append(" class=\"attention\""); // If element have minus sign add 'attention' css-class
                            }
                            htmlContentBuilder.AppendLine(string.Format(">{0}</td>", HttpUtility.HtmlEncode(row[counterRow]))); // Add element info
                        }
                        htmlContentBuilder.AppendLine("</tr>"); // Finish row
                    }
                    // Bottom of the table
                    htmlContentBuilder.AppendLine("<tr class=\"section__table__line last-column\">");
                    htmlContentBuilder.Append("<td colspan=\"6\">Итого сумма дебеторской задолженности покупателя ");
                    htmlContentBuilder.Append(HttpUtility.HtmlEncode(dataHtml.id));
                    htmlContentBuilder.AppendLine("</td>");
                    htmlContentBuilder.AppendLine("<td></td>");
                    htmlContentBuilder.Append("<td>");
                    dataHtml.sumTotal = TransformNumber(dataHtml.sumTotal);
                    htmlContentBuilder.Append(HttpUtility.HtmlEncode(dataHtml.sumTotal));
                    htmlContentBuilder.AppendLine("</td>");
                    htmlContentBuilder.Append("<td>");
                    htmlContentBuilder.Append(HttpUtility.HtmlEncode(dataHtml.currencyTotal));
                /*
                 * ----------------------------------------------
                */
                htmlContentBuilder.AppendLine("</td>");
                htmlContentBuilder.AppendLine("<td></td>");
                htmlContentBuilder.AppendLine("</tr>");
                htmlContentBuilder.AppendLine("</table>");
                htmlContentBuilder.AppendLine("</section>");
                htmlContentBuilder.AppendLine("<span>Спасибо.</span>");
                htmlContentBuilder.AppendLine("</body>");
                htmlContentBuilder.AppendLine("</html>");
            /*
             * -----------------------------------------------------------
            */
            string htmlContent = htmlContentBuilder.ToString(); // Transform builder into file struct

            try
            {
                string path = string.Format("KDDZ--{0}.html", FileParser.FileCreatedCounter + 1); // Creating new file full path
                using (StreamWriter htmlFile = new StreamWriter(path, false, System.Text.Encoding.Default)) // Try to create file
                {
                    FileParser.FileCreatedCounter++; // Increase counter
                    await htmlFile.WriteAsync(htmlContent); // Write info;
                }
            }
            catch (Exception e) // If exception...
            {
                MessageBox.Show(e.Message); // Show exception message
            }
        }
        
        static private string TransformNumber(string number) // Deleting in-value dot-delimiter and move minus sign to left
        {
            number = number.Replace(@".", @" "); // Replace dot-sign to space sign
            if (Regex.IsMatch(number, @"-")) // If number include minus sign
            {
                number = number.Replace(@"-", @""); // Remove minus sign
                number = '-' + number; // Add minus sign to begining
            }
            return number;
        }

    }
}
