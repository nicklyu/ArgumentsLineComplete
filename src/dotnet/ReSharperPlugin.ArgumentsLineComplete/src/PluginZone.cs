using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi.CSharp;

namespace ReSharperPlugin.ArgumentsLineComplete;

[ZoneDefinition]
public class ArgumentsLineZoneDefinition : IZone, IRequire<ILanguageCSharpZone>
{
}