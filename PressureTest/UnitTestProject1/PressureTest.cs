using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using NSoup;
using NSoup.Nodes;
using NSoup.Select;

namespace PressureTestProject1
{
    [TestClass]
    public class PressureTest
    {
        static IWebDriver driver;
        static String baseUrl;
        static String timeToFind;
        static TimeSpan timeToWait;

        WebDriverWait wait; //Used to wait for pages to load

        // Setup the environment
        public void Setup()
        {
            // Using IE but can be changed to Firefox.
            driver = new InternetExplorerDriver();
            //driver = new FirefoxDriver();
                        
            // BBC weather site
            baseUrl = "http://www.bbc.co.uk/weather/";

            // Allow the time to find to be changed
            // as sometimes 2100 is not avaialble
            timeToFind = "21 00";

            // Set the Max time to wait for an object to be ready
            // against using an instance of the WebDriverWait class.
            timeToWait = new TimeSpan(0, 0, 60); //30 Seconds
            wait = new WebDriverWait(driver, timeToWait);
        }


        // Helper method to check the element on the page is present, visible and enabled
        private Boolean IsElementPresent(By by)
        {
            // Wait for up to the timeout for the object to be visible
            // Then check it is enabled and displayed.
            IWebElement webElement = wait.Until(ExpectedConditions.ElementIsVisible(by));
            if (webElement.Displayed && webElement.Enabled)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // Helper method for the extraction of the pressure data from the table on the website
        // As this was a port from Java which used JSoup, this implementation uses NSoup as the HTML Parser.
        private int ExtractPressureData()
        {
            int timecolumnindex = 0;
            String pressureatgiventime = "Not Set";

            // Using NSoup HTMLParser, get the page and locate the weather
            // table
            Document document;

            try
            {
                // Load the page into the HTML parser
                IConnection conn = NSoup.Helper.HttpConnection.Connect(driver.Url);
                document = conn.Get();

                // Locate the weather table which holds the time and pressure data
                Elements TableElements = document.Select("table.weather");

                // Loop through the header to find the desired time
                // and record its column position
                Elements tableheaderelements = TableElements.Select("thead tr th");

                for (int i = 0; i < tableheaderelements.Count(); i++)
                {
                    if (tableheaderelements.ElementAt(i).Children.Text.StartsWith(timeToFind))
                    {
                        timecolumnindex = i - 1; // Take off 1 as the header text "Time"
                                                 // is included in the count, and no header
                                                 // text is included in the pressure row.
                    }
                }

                // Loop through the rest of the table rows until the Pressure
                // row is found,
                // then loop through the pressure data to find the value in the
                // same column as the desired time.
                Elements tablerowelements = TableElements.Select(":not(thead) tr");

                // row loop
                for (int i = 0; i < tablerowelements.Count(); i++)
                {
                    Element row = tablerowelements.ElementAt(i);

                    if (row.Text().StartsWith("Pressure"))
                    {
                        // Loop through the pressure data
                        // Need to take the value from the same column as the
                        // desired time
                        Elements rowitems = row.Select("td");
                        for (int j = 0; j < rowitems.Count(); j++)
                        {
                            if (j == timecolumnindex)
                            {
                                // Record this pressure
                                pressureatgiventime = rowitems.ElementAt(j).Text();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: catch block
            }

            return Convert.ToInt32(pressureatgiventime);
        }


        [TestMethod]
        // The test method that controls the activity of loading web pages and using
        // helper methods to check web page readiness and scraping the data from the tables.
        // Find the pressure today at a given time, find the pressure tomorrow at the same time
        // Subtract one from the other and send the result out to the console.
        public void TestWeatherPressure()
        {
            // Setup initial values
            int pressureday1 = 0;
            int pressureday2 = 0;
            String errormessage = "No Error";

            // Setup the environment
            Setup();

            // Load the page
            driver.Navigate().GoToUrl(baseUrl);

            // Proceed if the page has loaded and the search box is present
            if (IsElementPresent(By.Id("locator-form-search")))
            {
                // Search for Reading
                driver.FindElement(By.Id("locator-form-search")).Clear();
                driver.FindElement(By.Id("locator-form-search")).SendKeys("Reading, Reading");
                driver.FindElement(By.Id("locator-form-search")).SendKeys(Keys.Return);

                // Proceed if the Table detail link is available
                if (IsElementPresent(By.Id("detail-table-view")))
                {
                    // click on the table detail view to drop down the page
                    driver.FindElement(By.Id("detail-table-view")).Click();

                    //
                    // extract pressure for 21:00 from the table for today
                    //
                    pressureday1 = ExtractPressureData();

                    //
                    // Move to next day
                    //
                    driver.Navigate().GoToUrl(driver.Url + "?day=1");

                    // Check the page is ready by looking for the table
                    // detail link being present after the page update 
                    if (IsElementPresent(By.Id("detail-table-view")))
                    {
                        //
                        // extract pressure for the desired time for tomorrow
                        //
                        pressureday2 = ExtractPressureData();

                        // Output the result to the Test Console
                        if (pressureday1 == pressureday2)
                        {
                            Console.WriteLine("Pressure has not changed, remaining at " + pressureday1);
                        }
                        else
                        {
                            if (pressureday1 > pressureday2)
                            {
                                Console.WriteLine("Pressure decreased by " + (pressureday1 - pressureday2) + " from today "
                                        + pressureday1 + " to tomorrow " + pressureday2);
                            }
                            else
                            {
                                Console.WriteLine("Pressure increased by " + (pressureday2 - pressureday1) + " from today "
                                        + pressureday1 + " to tomorrow " + pressureday2);
                            }
                        }

                        Console.WriteLine("\n\n\n");
                    }
                    else
                    {
                        errormessage = "page not ready";
                    }
                }
                else
                {
                    errormessage = "detail-table-view not present";
                }
            }
            else
            {
                errormessage = "locator-form-search not present";
            }

            if (!errormessage.Equals("No Error"))
            {
                // Output error string if an error is set
                Console.WriteLine(errormessage);
            }

            // Close the browser
            driver.Close();
            driver.Quit();
        }
    }
}
