using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
    
namespace app
{
    class FileParser // Provides workflow to parsing files
    {
        static public uint FileCreatedCounter // Getter/Setter to count of collected file data
        {
            get { return _fileCreatedCounter; } // Get counter of created files
            set { _fileCreatedCounter++; } // Increase number of created files
        }
        static uint _fileCreatedCounter = 0; // Count of collected file data

        public class CollectedFileData // Container class storing info from files
        {
            public string id;
            public string name;
            public string sumTotal;
            public string currencyTotal;
            public List<string[]> rowLines = new List<string[]>(); // Create empty list of rows to 2nd table
            public string creditLimit;
            public string creditExposure;
            public string overBudget;
        }

        static public void ParseFiles(string pathF31, string pathFBL5N) // Parsing selected files and execute HtmlCreator
        {
            List<CollectedFileData> cachedData = new List<CollectedFileData>(); // FBL5N data cache storage
            try // Parse FBL5N file and save received info into cacheData
            {
                using (StreamReader fileFBL5N = new StreamReader(pathFBL5N)) // Try to open FBL5N file
                {
                    CollectedFileData receivedData = new CollectedFileData(); // Create object to new data entity of FBL5N file
                    string lineFBL5N; // New FBL5N file line
                    while ((lineFBL5N = fileFBL5N.ReadLine()) != null) // Read new line until file ends
                    {
                        if (Regex.IsMatch(lineFBL5N, @"[|]")) // Split line with separator if line contains separator
                        {
                            string[] rowElements = Regex.Split(lineFBL5N, @"[|]"); // Save spliting result into variable
                            if (rowElements.Length == 13) // If length equal 13
                            {
                                if ((rowElements[1].Trim() == "St") || (rowElements[1].Trim() == "*")) continue; // If line contain 'St' then skip the line
                                string[] rowLine = new string[10]; // Create row array to 2nd table
                                for (int it = 2; it < 12; it++) // From 2 to 11 position of splited line...
                                {
                                    rowLine[it - 2] = rowElements[it].Trim(); // Save elements into new row line
                                }
                                receivedData.rowLines.Add(rowLine); // Add new rowline into object of new data entity
                            }
                            else
                            {
                                receivedData.sumTotal = rowElements[2]; // Save sum
                                receivedData.currencyTotal = rowElements[3]; // Save currency
                                cachedData.Add(receivedData); // Add new data entity into cache
                            }
                        }
                        else if (Regex.IsMatch(lineFBL5N, @"Customer")) // If line contains 'Customer'...
                        {
                            receivedData = new CollectedFileData(); // Erase previously saved entity (relink with new object)
                            receivedData.id = Regex.Replace(lineFBL5N, @"Customer", @"").Trim(); // Save user id
                        }
                        else if (Regex.IsMatch(lineFBL5N, @"Name")) // If line contains 'Name'...
                        {
                            receivedData.name = Regex.Replace(lineFBL5N, @"Name", @"").Trim(); // Save user name
                        }
                    }
                }
            }
            catch (Exception e) // If rised an exception...
            {
                MessageBox.Show(e.Message); // Show exception message
            }
            try // Parse F31 file and create html-files if parsed F31 info compatible with cached info from FBL5N file
            {
                using (StreamReader fileF31 = new StreamReader(pathF31)) // Try to open F31 file
                {
                    string lineF31; // Variable to current file line
                    for (int it = 0; it < 6; it++) fileF31.ReadLine(); // Skip first 6 lines
                    while ((lineF31 = fileF31.ReadLine()) != null) // Read lines before file ends
                    {
                        string[] rowElements = Regex.Split(lineF31, @"[|]"); // Split the line to row elements
                        if (rowElements.Length != 13) continue; // If row length equal 13 then skip the line
                        CollectedFileData detectedDataHTML; // Object to new html data entity
                        if ((detectedDataHTML = cachedData.Find(x => x.id == rowElements[1].Trim())) != null) // If line id find in cache then...
                        {
                            detectedDataHTML.creditLimit = rowElements[4].Trim(); // Save credit limit
                            detectedDataHTML.creditExposure = rowElements[6].Trim(); // Save credit exposure
                            detectedDataHTML.overBudget = rowElements[8].Trim(); // Save over budget
                            HtmlCreator.CreateHtml(detectedDataHTML); // Execute async creating html pages
                        }
                    }
                }
            }
            catch (Exception e) // If rised an exception...
            {
                MessageBox.Show(e.Message); // Show exception message
            }
        }
    }

}