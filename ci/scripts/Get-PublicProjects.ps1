return @(
    [PSCustomObject]@{
        Name = "DotVVM.Core";
        Path = "src/Framework/Core"
    },
    [PSCustomObject]@{
        Name = "DotVVM";
        Path = "src/Framework/Framework"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Owin";
        Path = "src/Framework/Hosting.Owin"
    },
    [PSCustomObject]@{
        Name = "DotVVM.AspNetCore";
        Path = "src/Framework/Hosting.AspNetCore"
    },
    [PSCustomObject]@{
        Name = "DotVVM.CommandLine";
        Path = "src/Tools/CommandLine"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tools.StartupPerf";
        Path = "src/Tools/StartupPerfTester"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Api.Swashbuckle.AspNetCore";
        Path = "src/Api/Swashbuckle.AspNetCore"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Api.Swashbuckle.Owin";
        Path = "src/Api/Swashbuckle.Owin"
    },
    [PSCustomObject]@{
        Name = "DotVVM.HotReload";
        Path = "src/Tools/HotReload/Common"
    },
    [PSCustomObject]@{
        Name = "DotVVM.HotReload.AspNetCore";
        Path = "src/Tools/HotReload/AspNetCore"
    },
    [PSCustomObject]@{
        Name = "DotVVM.HotReload.Owin";
        Path = "src/Tools/HotReload/Owin"
    }
    [PSCustomObject]@{
        Name = "DotVVM.Testing";
        Path = "src/Framework/Testing"
    },
    [PSCustomObject]@{
        Name = "DotVVM.DynamicData";
        Path = "src/DynamicData/DynamicData"
    },
    [PSCustomObject]@{
        Name = "DotVVM.DynamicData.Annotations";
        Path = "src/DynamicData/Annotations"
    },
    [PSCustomObject]@{
        Name = "DotVVM.AutoUI";
        Path = "src/AutoUI/Core"
    },
    [PSCustomObject]@{
        Name = "DotVVM.AutoUI.Annotations";
        Path = "src/AutoUI/Annotations"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.ApplicationInsights";
        Path = "src/Tracing/ApplicationInsights"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.ApplicationInsights.AspNetCore";
        Path = "src/Tracing/ApplicationInsights.AspNetCore"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.ApplicationInsights.Owin";
        Path = "src/Tracing/ApplicationInsights.Owin"
    }
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.MiniProfiler.AspNetCore";
        Path = "src/Tracing/MiniProfiler.AspNetCore"
    },
    [PSCustomObject]@{
        Name = "DotVVM.Tracing.MiniProfiler.Owin";
        Path = "src/Tracing/MiniProfiler.Owin"
    }
)
