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
