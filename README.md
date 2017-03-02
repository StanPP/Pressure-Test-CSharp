# Pressure-Test-CSharp# 
Selenium Webdriver test

This is an excercise using Selenium WebDriver to:

  * Open the BBC Weather website (http://www.bbc.co.uk/weather)
  * Use the 'Find a Forecast' search for get the weather in 'Reading'
  * Expand the weather results to display Pressure (using the 'Table' option)
  * Obtain the pressure value for 21:00 for today
  * Obtain the pressure value for 21:00 for tomorrow
  * Subtract the two values and then 'echo' the result in Selenium - In this case it is to the Console


The code has been built and tested on Windows 10 Home edition.

It is written in C# and tested using Visual Studio Community Edition

The v1.0.0 code is specifically written for the Internet Explorere browser.


Improvements, observations & future investigations
--------------------------------------------------

Parameterise the input to support other browsers.  Currently IE only, have tried it with Firefox and appeared ok.

Consider using the URL for Reading (http://www.bbc.co.uk/weather/2639577) rather than executing a search each time.

After 21:00 in the evening, this value disappears from the page for the current day, so I've added flexibility to allow this to be changed.  This could be an issue if this test were being run on an automated system late in the evening.

***Important Note***
Visual Studio must be Run As Administrator when using IE.
