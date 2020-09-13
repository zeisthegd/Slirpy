using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Settings : Node
{
    bool fullscreen = true;

    public override void _Ready()
    {
        
    }

    private void LoadSettings()
    {
        //var savedSettings = new File();
        //if(savedSettings)
        //var error = f.Open("user://settings.json", File.ModeFlags.Read);
        //if(error != Error.Ok)
        //{
        //    GD.Print("There's no settings to load!");
        //    return;
        //}

        //var d = JSON.Parse(f.GetAsText());
        //if (d.GetType() != typeof(Dictionary))
        //    return;

        //var settingsData = new Godot.Collections.Dictionary<string, object>(d.Result);

        //foreach(KeyValuePair<object,object> entry in )

    }
}
