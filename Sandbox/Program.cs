using Sandbox;
using StatimUI;

CSParser.ParseScript(@"
public int b = 2; 
private string c = ""2"";

public int double(int input)
{
    return 2 * input;
}
");

Sandbox.Window window = new Sandbox.Window();
window.Run();