using FloridaUccSearch;

//creates search object and sets the download folder
FlSearch a = new FlSearch("/Users/rstuard/Downloads/UCCImages");

//searches debtor name
a.Search("The Red Alamo Corp");

Console.WriteLine("done!");