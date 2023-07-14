return @(
    [PSCustomObject]@{
        Name = "DotVVM.Core";
        Path = "src/Framework/Core";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM";
        Path = "src/Framework/Framework";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Owin";
        Path = "src/Framework/Hosting.Owin";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.AspNetCore";
        Path = "src/Framework/Hosting.AspNetCore";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.CommandLine";
        Path = "src/Tools/CommandLine";
        Type = "tool"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Templates";
        Path = "src/Templates/DotVVM.Templates.nuspec";
        Type = "template"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tools.StartupPerf";
        Path = "src/Tools/StartupPerfTester";
        Type = "tool"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Api.Swashbuckle.AspNetCore";
        Path = "src/Api/Swashbuckle.AspNetCore";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Api.Swashbuckle.Owin";
        Path = "src/Api/Swashbuckle.Owin";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.HotReload";
        Path = "src/Tools/HotReload/Common";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.HotReload.AspNetCore";
        Path = "src/Tools/HotReload/AspNetCore";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.HotReload.Owin";
        Path = "src/Tools/HotReload/Owin";
        Type = "standard"
    }
    [PSCustomObject]@{
        Name = "DotVVM.Testing";
        Path = "src/Framework/Testing";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.DynamicData";
        Path = "src/DynamicData/DynamicData";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.DynamicData.Annotations";
        Path = "src/DynamicData/Annotations";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.AutoUI";
        Path = "src/AutoUI/Core";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.AutoUI.Annotations";
        Path = "src/AutoUI/Annotations";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.ApplicationInsights";
        Path = "src/Tracing/ApplicationInsights";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.ApplicationInsights.AspNetCore";
        Path = "src/Tracing/ApplicationInsights.AspNetCore";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.ApplicationInsights.Owin";
        Path = "src/Tracing/ApplicationInsights.Owin";
        Type = "standard"
    }
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.MiniProfiler.AspNetCore";
        Path = "src/Tracing/MiniProfiler.AspNetCore";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.MiniProfiler.Owin";
        Path = "src/Tracing/MiniProfiler.Owin";
        Type = "standard"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Adapters.WebForms";
        Path = "src/Adapters/WebForms";
        Type = "standard"
    }
)
