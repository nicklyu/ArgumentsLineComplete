using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly : Apartment(System.Threading.ApartmentState.STA)]

namespace ReSharperPlugin.ArgumentsLineComplete.Tests.test.Src
{

    [ZoneDefinition]
    public class ArgumentLineCompletionTestZone : ITestsEnvZone
    {
    }

    [ZoneActivator]
    public class PsiFeatureTestZoneActivator : IActivate<PsiFeatureTestZone>, IActivate<ArgumentsLineZoneDefinition>
    {
    }

    [ZoneActivator]
    public class SinceClr4HostZoneActivator : IActivate<ISinceClr4HostZone>
    {
    }

    [SetUpFixture]
    public class PsiFeaturesTestEnvironmentAssembly : ExtensionTestEnvironmentAssembly<ArgumentLineCompletionTestZone>
    {
    }
}