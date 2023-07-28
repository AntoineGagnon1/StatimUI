using Sandbox;
using StatimUI;

Statim.LoadEmbedded();

//Sandbox.Window window = new Sandbox.Window();
StatimParser.Parse(@"
<test hey=""allo"">
    <foreach {item} in {items}>
        <if {item ==""cool""}>
            <text value={you are cool}/>
        </if>
        <if {item == ""not cool""}>
            <text value={bind you.are.not.cool}/>
        </if>
    </foreach>
</test>");
//window.Run();
//Component.Width = new Dimension(new Property<float>());