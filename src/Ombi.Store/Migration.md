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


    docker run -d \
	--name some-postgres \
	-e POSTGRES_PASSWORD=ombi \
    -e POSTGRES_USER=ombi \
 	-e POSTGRES_DB=ombi \
	postgres

### MySql example
```
{
  "OmbiDatabase": {
    "Type": "MySQL",
    "ConnectionString": "Server=192.168.68.118;Port=3306;Database=ombiNew;User=ombi"
  },
  "SettingsDatabase": {
    "Type": "MySQL",
    "ConnectionString": "Server=192.168.68.118;Port=3306;Database=ombiNew;User=ombi"
  },
  "ExternalDatabase": {
    "Type": "MySQL",
    "ConnectionString": "Server=192.168.68.118;Port=3306;Database=ombiNew;User=ombi"
  }
}
```

### MSSQL Example
```
{
  "OmbiDatabase": {
    "Type": "MSSQL",
    "ConnectionString": "Server=localhost;Database=ombi;User Id=ombi;Password=ombi;TrustServerCertificate=True"
  },
  "SettingsDatabase": {
    "Type": "MSSQL",
    "ConnectionString": "Server=localhost;Database=ombi;User Id=ombi;Password=ombi;TrustServerCertificate=True"
  },
  "ExternalDatabase": {
    "Type": "MSSQL",
    "ConnectionString": "Server=localhost;Database=ombi;User Id=ombi;Password=ombi;TrustServerCertificate=True"
  }
}
```

### Postgres Example
```
{
  "OmbiDatabase": {
    "Type": "Postgres",
    "ConnectionString": "Host=localhost;Port=5432;Database=ombi;Username=ombi;Password=ombi"
  },
  "SettingsDatabase": {
    "Type": "Postgres",
    "ConnectionString": "Host=localhost;Port=5432;Database=ombi;Username=ombi;Password=ombi"
  },
  "ExternalDatabase": {
    "Type": "Postgres",
    "ConnectionString": "Host=localhost;Port=5432;Database=ombi;Username=ombi;Password=ombi"
  }
}
```