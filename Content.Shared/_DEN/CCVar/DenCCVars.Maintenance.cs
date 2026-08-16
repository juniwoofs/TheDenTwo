using Robust.Shared.Configuration;

namespace Content.Shared._DEN.CCVar;

public sealed partial class DenCCVars
{
    /// <summary>
    ///     When enabled, only admins can join the server.
    /// </summary>
    public static readonly CVarDef<bool> MaintenanceModeEnabled =
        CVarDef.Create("den.maintenance.enabled", false, CVar.SERVERONLY);
}
