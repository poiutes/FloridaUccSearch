namespace FloridaUccSearch;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


public class FlSearch
{
    //Selenium Driver
    private IWebDriver driver;

    //Folder where are images will be downloaded
    string downloadFolderDest;

    //FlSearch Constructor
    public FlSearch(string downloadFolderInput)
    {
        //Sets what folder our images will be downloaded to
        downloadFolderDest = downloadFolderInput;
        var driverOptions = new ChromeOptions();
        driverOptions.AddUserProfilePreference("download.default_directory", downloadFolderDest);
        driver = new ChromeDriver(driverOptions);

        //opens up florida search and accepts the terms and conditions before performing
        //a searh
        driver.Navigate().GoToUrl(@"https://www.floridaucc.com/uccweb/search.aspx");
        driver.FindElement(By.Id("AcceptCheckbox")).Click();
        driver.FindElement(By.Id("nextButton")).Click();
    }

    //Counts the number of files in a directory - needed later to check that we're
    //finished downloading an image before downloading the next
    public static int CountFiles(string dir)
    {
        int count = 0;
        string[] files = Directory.GetFiles(dir);
        count = files.Length;
        return count;
    }

    //Ends the selenium session
    public void End()
    {
        driver.Quit();
        Console.WriteLine("done");
    }

    //Where the actual search and download happens
    public void Search(string debtorName)
    {
        //changes the search name to upper case and strips punctution 
        debtorName = debtorName.ToUpper();
        string strippedSearch = string.Concat(debtorName.Where(ch => Char.IsLetterOrDigit(ch)));

        //goes to flordida search page and puts in our search name and hits enter
        driver.Navigate().GoToUrl(@"https://www.floridaucc.com/uccweb/search.aspx");
        var searchBox = driver.FindElement(By.Id("SearchTextTextBox"));
        searchBox.Clear();
        searchBox.SendKeys(debtorName);
        searchBox.SendKeys(Keys.Enter);

        //checks if our search has any hits -> if there are hits we add the links to their
        //results page to the "resultLinksFinal" list
        List<string> resultLinksFinal = new List<string>();
        while (true)
        {
            //turns table of results into a list
            IReadOnlyList<IWebElement> results = driver.FindElement(By.Id("SearchResultsGridView")).FindElements(By.TagName("tr"));

            //checks each result for a match
            List<string> resultLinks = new List<string>();
            for (int i = 1; i < results.Count; i++)
            {
                //grabs a results name and removes puncuation
                IWebElement resultLine = results[i].FindElement(By.TagName("a"));
                string strippedResultLine = string.Concat(resultLine.Text.Where(ch => Char.IsLetterOrDigit(ch)));

                //checks if it's the same as our search and adds to our temporary results list (resultLinks)
                if (strippedSearch == strippedResultLine)
                {
                    resultLinks.Add(resultLine.GetAttribute("href"));
                }

            }
            //adds every link we found to "resultLinksFinal"
            resultLinksFinal.AddRange(resultLinks);

            //if we ever find anything, we always check the next page for more results
            //breaks the loop if we didn't find anything
            if (resultLinks.Count == 0)
            {
                break;
            }
            else
            {
                driver.FindElement(By.Id("ButtonNext")).Click();
            }
        }


        //goes through all the result pages and downloads the images
        foreach (string x in resultLinksFinal)
        {
            //goes to results page
            driver.Navigate().GoToUrl(x);

            //clicks on the UCC1 download link at the bottom of the page
            int preCheckFolderCount = CountFiles(downloadFolderDest);
            driver.FindElement(By.Id("DocumentImagesGridView")).FindElement(By.TagName("a")).Click();

            //waits for file to appear in folder before proceeding
            while (true)
            {
                if (preCheckFolderCount < CountFiles(downloadFolderDest))
                {
                    break;
                }
            }

            //checks if results page has any filing events (ucc3s)
            IReadOnlyList<IWebElement> filingHistoryLink = driver.FindElements(By.Id("EventsLink2"));
            if (filingHistoryLink.Count > 0)
            {
                filingHistoryLink[0].Click();

                // go to last page of filing events before downloading (so we download ucc3s in order - earliest filings on last page)
                while (true)
                {
                    //checks if there's another page of ucc3s - if yes go to next page, if not break loop
                    IReadOnlyList<IWebElement> filingHistoryNext = driver.FindElements(By.Id("NextButton"));
                    if (filingHistoryNext.Count > 0)
                    {
                        filingHistoryNext[0].Click();

                    }
                    else
                    {
                        break;
                    }
                }

                //once we're on the last page of ucc3s, we can start downloading ucc3s
                while (true)
                {
                    //gets the download links
                    IReadOnlyList<IWebElement> ucc3s = driver.FindElement(By.ClassName("text-area")).FindElements(By.TagName("a"));

                    //downloads them in reverse order since earliest uccs3 are last in the list/bottom of the page
                    for (int i = ucc3s.Count - 1; i >= 0; i--)
                    {
                        preCheckFolderCount = CountFiles(downloadFolderDest);

                        //click download link
                        ucc3s[i].Click();

                        //wait for download to finish
                        while (true)
                        {
                            if (preCheckFolderCount < CountFiles(downloadFolderDest))
                            {
                                break;
                            }
                        }

                    }

                    //checks if there's another prev page of ucc3s - if yes, go there
                    //if not break loop - if the loop breaks, then we should have downloaded all the images
                    //associated with this results page in order
                    IReadOnlyList<IWebElement> filingHistoryPrev = driver.FindElements(By.Id("PreviousButton"));

                    if (filingHistoryPrev.Count > 0)
                    {
                        filingHistoryPrev[0].Click();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


    }
}