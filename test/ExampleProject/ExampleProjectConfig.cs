﻿namespace ExampleProject;

using ConfigManager.Attributes;

[Config]
public partial class ExampleProjectConfig
{
    private string projectVersion;
    private string projectName;
    private string projectType;
    
    // [ConfigIgnore]
    // private string someIgnoredValue;

    // public string ProjectString => $"{ProjectVersion}:{ProjectName}";
}