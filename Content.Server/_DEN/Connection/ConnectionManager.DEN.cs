using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._DEN.CCVar;
using Robust.Shared.Network;

namespace Content.Server.Connection;

public sealed partial class ConnectionManager
{
    private async Task<(ConnectionDenyReason, string)?> DenShouldDeny(NetConnectingArgs e, Admin? adminData)
    {
        var maintenanceModeEnabled = _cfg.GetCVar(DenCCVars.MaintenanceModeEnabled);

        if (!maintenanceModeEnabled || adminData != null)
            return null;

        return (ConnectionDenyReason.Whitelist, Loc.GetString("maintenance-mode-disconnect"));
    }
}
