
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using RestSharp;

Console.Write("Address: ");
var addr = Console.ReadLine();

Console.Write("itchio cookie: ");
var itchio = Console.ReadLine();

Console.Write("itchio_token cookie: ");
var itchioToken = Console.ReadLine();

var cookiesHeader = $"itchio={itchio}; itchio_token={itchioToken}";
var client = new RestClient();
var color = Console.ForegroundColor;
var preReq = new RestRequest(addr, Method.Get);
preReq.AddHeader("Cookie", cookiesHeader);
var preResp = await client.ExecuteAsync(preReq);
var preHd = new HtmlDocument();
preHd.LoadHtml(preResp.Content);
var pagerString = preHd.DocumentNode.SelectSingleNode("//span[@class='pager_label']/a").GetAttributeValue("href", "0");
var pagerRegex = new Regex(@"\d+");
var pages = int.Parse(pagerRegex.Match(pagerString).Value);
Console.WriteLine($"Found {pages} pages");
for(int i = 1; i <= pages; ++i) {
    var r = new RestRequest(addr + $"?page={i}", Method.Get);
    r.AddHeader("Cookie", cookiesHeader);
    var response = await client.ExecuteAsync(r);
    var byForm = 0;
    var hd = new HtmlDocument();
    hd.LoadHtml(response.Content);
    
    var buttons = hd.DocumentNode.SelectNodes("//button[@value='claim']");
    if(buttons != null)
        foreach(var b in buttons){
            var form = b.ParentNode;
            var csrfToken = form.SelectSingleNode("input[@name='csrf_token']").Attributes["value"].Value;
            var gameId = form.SelectSingleNode("input[@name='game_id']").Attributes["value"].Value;
            var action = "claim";
            var data = $"csrf_token={Uri.EscapeDataString(csrfToken)}&game_id={Uri.EscapeDataString(gameId)}&action={action}";
            Console.WriteLine($"Claiming {data}");

            var r3 = new RestRequest(addr, Method.Post);
            r3.AddHeader("Cookie", cookiesHeader);
            r3.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            r3.AddStringBody(data, DataFormat.None);
            var response3 = await client.ExecuteAsync(r3);
            if(response3.StatusCode == System.Net.HttpStatusCode.OK)
                byForm++;
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error while claiming {data}");
                Console.ForegroundColor = color;
            }
        }
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Page {i}: {byForm} total claimed");
    Console.ForegroundColor = color;
    /*var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Page {i}");
    Console.ForegroundColor = color;
    Console.WriteLine(response.Content);*/
}