using System;
using System.Runtime.CompilerServices;

namespace Ombi.Store.Context.Postgres;

public static class PostgresModuleInitializer
{
#pragma warning disable CA2255
    // This is required to ensure that Npgsql uses a timestamp behavior that does not require a timezone
    // Reference: https://stackoverflow.com/a/73586129
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}