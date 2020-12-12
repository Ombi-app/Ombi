```
dotnet ef migrations add Inital --context OmbiSqliteContext --startup-project ../Ombi/Ombi.csproj
```

If running migrations for any db provider other than Sqlite, then ensure the database.json is pointing at the correct DB type


## More detailed explanation

1. Install dotnet-ef, and include it in your $PATH if necessary:

    ```
    dotnet tool install --global dotnet-ef
    export PATH="$HOME/.dotnet/tools:$PATH"
    ```

1. In `src/Ombi`, install the `Microsoft.EntityFrameworkCore.Design` package:

    ```
    cd src/Ombi
    dotnet add package Microsoft.EntityFrameworkCore.Design
    ```

1. For some reason, the `StartupSingleton.Instance.SecurityKey` in `src/Ombi/Extensions/StartupExtensions.cs` is invalid when running `otnet ef migrations add` so we must fix it; apply this patch which seems to do the job:

    ```
    @@ -79,7 +79,7 @@ namespace Ombi
                 var tokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
    -                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(StartupSingleton.Instance.SecurityKey)),
    +                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(StartupSingleton.Instance.SecurityKey + "s")),
                     RequireExpirationTime = true,
                     ValidateLifetime = true,
                     ValidAudience = "Ombi",
    ```

1. List the available `dbcontext`s, and select the one that matches the database your fields will go in:

    ```
    cd src/Ombi.Store
    dotnet ef dbcontext list
    ```

1. Run the migration using the command at the start of this document: 

    ```
    cd src/Ombi.Store
    dotnet ef migrations add <name> --context <context> --startup-project ../Ombi/Ombi.csproj
    ```
