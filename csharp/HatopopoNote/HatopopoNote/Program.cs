using HatopopoNote;

var markdownConverter = new MarkdownConverter();
var res = await markdownConverter.Execute();
if(!res) Console.WriteLine("Convert failed.");