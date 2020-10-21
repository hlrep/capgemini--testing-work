using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace app
{
    class Program
    {
        static uint htmlFileCounter = 0;

        static void Main(string[] args)
        {
            const string pathF31 = "F31.txt";
            const string pathFBL5N = "FBL5N.txt";

            Parser(pathF31, pathFBL5N);
            MessageBox.Show(string.Format("В ходе работы создан(о) {0} файл(ов)", htmlFileCounter));
        }

        class DataHTML
        {
            public string id;
            public string name;
            public string sumTotal;
            public string currencyTotal;
            public List<string[]> rowLines = new List<string[]>();
            public string creditLimit;
            public string creditExposure;
            public string overBudget;
        }

        static void Parser(string pathF31, string pathFBL5N)
        {
            List<DataHTML> cachedData = new List<DataHTML>();
            // Парсим файл FBL5N.txt и загоняем нужные данные в кэш в виду небольшого объёма
            try
            {
                using (StreamReader fileFBL5N = new StreamReader(pathFBL5N))
                {
                    DataHTML receivedData = new DataHTML();
                    string lineFBL5N;
                    while ((lineFBL5N = fileFBL5N.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(lineFBL5N, @"[|]"))
                        {
                            string[] rowElements = Regex.Split(lineFBL5N, @"[|]");
                            if (rowElements.Length == 13)
                            {
                                if ((rowElements[1].Trim() == "St") || (rowElements[1].Trim() == "*")) continue;
                                string[] rowLine = new string[10];
                                for (int it = 2; it < 12; it++)
                                {
                                    rowLine[it - 2] = rowElements[it].Trim();
                                }
                                receivedData.rowLines.Add(rowLine);
                            }
                            else
                            {
                                receivedData.sumTotal = rowElements[2];
                                receivedData.currencyTotal = rowElements[3];
                                cachedData.Add(receivedData);
                            }
                        }
                        else if (Regex.IsMatch(lineFBL5N, @"Customer"))
                        {
                            receivedData = new DataHTML();
                            receivedData.id = Regex.Replace(lineFBL5N, @"Customer", @"").Trim();
                        }
                        else if (Regex.IsMatch(lineFBL5N, @"Name"))
                        {
                            receivedData.name = Regex.Replace(lineFBL5N, @"Name", @"").Trim();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            // Парсим файл F31.txt и при совпадениях создаём HTML-файлы с собранными данными
            try
            {
                using (StreamReader fileF31 = new StreamReader(pathF31))
                {
                    string lineF31;
                    for (int it = 0; it < 6; it++) fileF31.ReadLine();
                    while ((lineF31 = fileF31.ReadLine()) != null)
                    {
                        string[] rowElements = Regex.Split(lineF31, @"[|]");
                        if (rowElements.Length != 13) continue;
                        DataHTML detectedDataHTML;
                        if ((detectedDataHTML = cachedData.Find(x => x.id == rowElements[1].Trim())) != null)
                        {
                            detectedDataHTML.creditLimit = rowElements[4].Trim();
                            detectedDataHTML.creditExposure = rowElements[6].Trim();
                            detectedDataHTML.overBudget = rowElements[8].Trim();
                            CreateHTML(detectedDataHTML);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        static async void CreateHTML(DataHTML dataHTML)
        {
            StringBuilder htmlContentBuilder = new StringBuilder();

            string date = DateTime.Now.ToString("dd.mm.yyyy");

            htmlContentBuilder.AppendLine("<!DOCTYPE html>");
            htmlContentBuilder.AppendLine("<html lang=\"ru\" dir=\"ltr\">");
            htmlContentBuilder.AppendLine("<head>");
            htmlContentBuilder.AppendLine("<meta charset=\"utf - 8\">");
            htmlContentBuilder.AppendLine(string.Format("<title>КЛиДЗ#{0}</title>", HttpUtility.HtmlEncode(htmlFileCounter + 1)));
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
                ".attention { background-color: lightgreen; }");
            htmlContentBuilder.AppendLine("</style>");
            htmlContentBuilder.AppendLine("<section class=\"section\">");
            htmlContentBuilder.Append("<h2 class=\"section__title\">Кредитный лимит на ");
            htmlContentBuilder.Append(HttpUtility.HtmlEncode(date));
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
            // Данные первой таблицы
            htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHTML.id)));
            htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHTML.name)));
            dataHTML.creditLimit = TransformNumber(dataHTML.creditLimit);
            htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHTML.creditLimit)));
            dataHTML.creditExposure = TransformNumber(dataHTML.creditExposure);
            htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHTML.creditExposure)));
            dataHTML.overBudget = TransformNumber(dataHTML.overBudget);
            htmlContentBuilder.AppendLine(string.Format("<td>{0}</td>", HttpUtility.HtmlEncode(dataHTML.overBudget)));
            // ---------------------
            htmlContentBuilder.AppendLine("</tr>");
            htmlContentBuilder.AppendLine("</table>");
            htmlContentBuilder.AppendLine("</section>");
            htmlContentBuilder.AppendLine("<section class=\"section\">");
            htmlContentBuilder.Append("<h2 class=\"section__title\">Дебиторская задолженность на ");
            htmlContentBuilder.Append(HttpUtility.HtmlEncode(date));
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
            // Данные второй таблицы
            foreach (string[] row in dataHTML.rowLines)
            {
                htmlContentBuilder.AppendLine("<tr class=\"section__table__line\">");
                row[6] = TransformNumber(row[6]);
                row[7] = TransformNumber(row[7]);
                for (int counterRow = 0; counterRow < row.Length; counterRow++)
                {
                    htmlContentBuilder.Append("<td");
                    if (counterRow == 6)
                    {
                        if (!Regex.IsMatch(row[6], @"-")) htmlContentBuilder.Append(" class=\"attention\"");
                    }
                    htmlContentBuilder.AppendLine(string.Format(">{0}</td>", HttpUtility.HtmlEncode(row[counterRow])));
                }
                htmlContentBuilder.AppendLine("</tr>");
            }
            // ---------------------
            htmlContentBuilder.AppendLine("<tr class=\"section__table__line last-column\">");
            htmlContentBuilder.Append("<td colspan=\"6\">Итого сумма дебеторской задолженности покупателя ");
            // Идентификатор пользователя
            htmlContentBuilder.Append(HttpUtility.HtmlEncode(dataHTML.id));
            // --------------------------
            htmlContentBuilder.AppendLine("</td>");
            htmlContentBuilder.AppendLine("<td></td>");
            htmlContentBuilder.Append("<td>");
            // Сумма задолженности
            dataHTML.sumTotal = TransformNumber(dataHTML.sumTotal);
            htmlContentBuilder.Append(HttpUtility.HtmlEncode(dataHTML.sumTotal));
            // -------------------
            htmlContentBuilder.AppendLine("</td>");
            htmlContentBuilder.Append("<td>");
            // Тип валюты
            htmlContentBuilder.Append(HttpUtility.HtmlEncode(dataHTML.currencyTotal));
            // -------------------
            htmlContentBuilder.AppendLine("</td>");
            htmlContentBuilder.AppendLine("<td></td>");
            htmlContentBuilder.AppendLine("</tr>");
            htmlContentBuilder.AppendLine("</table>");
            htmlContentBuilder.AppendLine("</section>");
            htmlContentBuilder.AppendLine("<span>Спасибо.</span>");
            htmlContentBuilder.AppendLine("</body>");
            htmlContentBuilder.AppendLine("</html>");

            string htmlContent = htmlContentBuilder.ToString();

            try
            {
                string path = string.Format("KDDZ--{0}.html", ++htmlFileCounter);
                using (StreamWriter htmlFile = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    await htmlFile.WriteAsync(htmlContent);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        static private string TransformNumber(string number)
        {
            number = number.Replace(@".", @" ");
            if (Regex.IsMatch(number, @"-"))
            {
                number = number.Replace(@"-", @"");
                number = '-' + number;
            }
            return number;
        }
    }
}
