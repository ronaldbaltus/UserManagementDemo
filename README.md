# User Management Demo
This demo uses [ASP.NET Core](https://dotnet.microsoft.com/download), [MariaDB](https://mariadb.org) and [EventStore](https://eventstore.com/) to make a simple User Management website. MariaDB stores the current state of the users, while the EventStore has a log of all events of the users.

## How to run?

### Docker-compose
`docker-compose up`

- The webserver is available on port 8000
- The EventStore admin Web UI is available on port 2113 (admin:changeit)
- The MariaDB (MySQL) is available on port 3306 (This is not a Web interface)

### Directly from Visual Studio

Change the connection strings within the `appsettings.json` to connect to MariaDB and EventStore instances. For EventStore make sure to use version 5.0.8. I was unable to get version 20.6.0 to work with .NET Core.