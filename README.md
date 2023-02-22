# FloridaUccSearch

A library for fetching ucc documents on the Florida UCC website.

Below is the example used in the FlSearchExample folder.

```
using FloridaUccSearch;

//creates search object and sets the download folder (set to your own)
FlSearch a = new FlSearch("/Users/rstuard/Downloads/UCCImages");

//searches debtor name
a.Search("The Red Alamo Corp");

Console.WriteLine("done!");
```

After this code snippet runs, you should have the ucc images downloaded in order in your designated download folder. 
