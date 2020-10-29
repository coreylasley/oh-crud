### Adding Language/Framework Support

A new Class that inherits from the Abstract Class [BackEndCode](backendcode.html) (which implements the ICode interface) can be created. The Class should be named to best represent its language/framework.

### Language/Framework Enum
In order for the new Language/Framework to appear on the Codeterpret app, it must have an enum type added to *Codeterpret > Common > Enums > BackendProjectTypes* 

### Defining a Settings Form
The implementation of the **GenerateProject()** method for a given language/framework is clearly not going to be the same. Different languages/frameworks require different pieces of information in order to generate properly working code. Thus, outside of the standard parameters, the **GenerateProject()** method accepts a *SettingGroup* object which hold values for any number of additional custom properties that might be needed in order to properly generate code. This object also defines how such values are obtained from the Codeterpret user. 

The following code block is an example *SettingGroup* definition, and should be fairly intuitive. This definition contains all of the currently supported element Types (*Text, Select, Check, Label, and LineBreak*) and demonstrates  controlling the flow and how certain setting elements can be displayed/hidden based on the values of other setting elements.
```csharp  
SettingGroup sg = new SettingGroup();

sg.Settings.Add(new Setting { Type = InputTypes.Check, Key = "WantToJoin", Display = true, Label = "Do you want to join the Gamer's Group?" });
sg.Settings.Add(new Setting { Type = InputTypes.LineBreak });

sg.Settings.Add(new Setting { Type = InputTypes.Text, Key = "FirstName", Label = "First Name", OnlyDisplayWhenKey = "WantToJoin", OnlyDisplayWhenValue = "true" });
sg.Settings.Add(new Setting { Type = InputTypes.Text, Key = "LastName", Label = "Last Name", OnlyDisplayWhenKey = "WantToJoin", OnlyDisplayWhenValue = "true" });
sg.Settings.Add(new Setting { Type = InputTypes.LineBreak });

List<SettingOption> so = new List<SettingOption>();
so.Add(new SettingOption { Value = "1", Label = "XBox" });
so.Add(new SettingOption { Value = "2", Label = "PlayStation" });
so.Add(new SettingOption { Value = "3", Label = "Switch" });
so.Add(new SettingOption { Value = "4", Label = "PC" });
so.Add(new SettingOption { Value = "5", Label = "Classic NES" });
sg.Settings.Add(new Setting { Type = InputTypes.Select, Key = "Console", Label = "System of Choice", Options = so, OnlyDisplayWhenKey = "WantToJoin", OnlyDisplayWhenValue = "true" });

sg.Settings.Add(new Setting { Type = InputTypes.LineBreak });
sg.Settings.Add(new Setting { Type = InputTypes.Label, Key = "", Label = "More Info Requested", Value = "We like your choice, so we would like a little more info.", OnlyDisplayWhenKey = "Console", OnlyDisplayWhenValue = "2,5", OnlyDisplayWhenValueMultiValues = true });
sg.Settings.Add(new Setting { Type = InputTypes.LineBreak });

sg.Settings.Add(new Setting { Type = InputTypes.Text, Key = "NESExtraQuestion", Label = "Oh Yeah! What is your favorite game?", OnlyDisplayWhenKey = "Console", OnlyDisplayWhenValue = "5" });

so = new List<SettingOption>();
so.Add(new SettingOption { Value = "1", Label = "PS4" });
so.Add(new SettingOption { Value = "2", Label = "PS2" });
so.Add(new SettingOption { Value = "3", Label = "PS1" });
sg.Settings.Add(new Setting { Type = InputTypes.Select, Key = "PSExtraQuestion", Label = "What Flavor of PlayStation?", Options = so, OnlyDisplayWhenKey = "Console", OnlyDisplayWhenValue = "2" }); 
```
Because the app uses [Blazorise](https://blazorise.com) for styling, elements are grouped together in [Fields](https://blazorise.com/docs/components/field/#fields) blocks, to best utilize screen real-estate. In order to force element rendering onto the next line, there is the InputType.LineBreak. 