using Sandbox;
using StatimUI;

Statim.LoadEmbedded();

Sandbox.Window window = new Sandbox.Window();
StatimParser.Parse("<test hey=\"allo\">\r\n    <foreach {item} in {fucking_elements}>\r\n        hello\r\n    </foreach>\r\n</test>");
window.Run();
//Component.Width = new Dimension(new Property<float>());