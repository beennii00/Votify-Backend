# Votify-Push-Pray

### Comandos para ejecutar
``` sh 
dotnet restore
dotnet run --project API
dotnet run --project Web
```

### Comando para encender la base de datos
```sh
dotnet ef database update --project Votify.Persistance --startup-project API
```

Si no funciona, se puede crear el rol con postgres
```sh
psql postgres

postgres=# UPDATE ROLE postgres WITH LOGIN SUPERUSER PASSWORD '1234'
```
