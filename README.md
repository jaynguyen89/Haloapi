# Haloapi: .NET Web API

This project is to develop the core api services that will be consumed by the **<span style="color: orange;">Haloweb</span>** application. This document provides the information 
that are necessary for initial setup, running, developing and maintaining the API. Please read through before putting hands on working.

### 1. Prerequisites
The following software and tools should be set up to prepare for the next step of configuring the system to run the API:
- Please say a big NO to using Visual Studio Code as your primary IDE to simplify things, leave your brain for complex things that you will find soon later :smirk:
- SDK: .NET 6 and C# 10 - downloadable from https://dotnet.microsoft.com/en-us/download
- IDE:
  - Jetbrains Rider 2022 or later is preferable.
  - Alternative: Visual Studio 2019 or later. https://visualstudio.microsoft.com/downloads/
- Database tools:
  - MSSQL Server 2019 or later, Express edition is sufficient. https://www.microsoft.com/en-us/Download/details.aspx?id=101064
  - SQL Server Management Studio (SSMS). https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16
  - (Windows) Install LocalDB along with MSSQL Server.
  - Xampp server: download here https://www.apachefriends.org/download.html
  - dbForge Studio or MySQL Workbench: https://dev.mysql.com/downloads/workbench/
  - MongoDB Compass: https://www.mongodb.com/try/download/compass
  - AWS NoSQL Workbench: https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/workbench.settingup.html
  - For MacOS:
    - Azure Data Studio is preferred. https://learn.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio?view=sql-server-ver16&tabs=redhat-install%2Credhat-uninstall
    - Jetbrains DataGrip (handy tool to manage database)
- Docker: install Docker and enable Kubernetes. https://www.docker.com/products/docker-desktop/
- Testing tool:
  - Postman is preferred: https://www.postman.com/downloads/
  - Alternative: Advanced REST Client https://github.com/advanced-rest-client/arc-electron/releases
- Redis server:
  - Both Windows and MAC user can use the downloaded archive here: https://redis.io/download/
- For MacOS:
  - Install Homebrew: read the instruction here https://brew.sh/
  - Use Homwbrew to install Redis server (instead of doing the step above)

### 2. Preparing local environment
The API supports 4 environments: Local, Development, Staging and Production, in each of which it may behave a bit differently in some of its services. To run the app on your 
local computer, you need to prepare these things:
- Add the following 5 variables into your local environment variables:
  - Halogen_Environment=Local
  - Halogen_UseLongerId=False
  - Halogen_AwsAccessKeyId=<Your AWS Access Key ID>
  - Halogen_AwsSecretAccessKey=<Your AWS Secret Access Key>
  - Halogen_AwsRegion=us-east-1
- In MSSQL Server, create database name: HalogenDatabase
- In Postman or Advanced REST Client, import the collections according to the tool you use, please find the collection export in our assets directory.
- On MacOS, use Homebrew to pull the following images:
  - mongodb/mongodb-community-server: https://www.mongodb.com/docs/manual/tutorial/install-mongodb-community-with-docker/
  - redis: https://hub.docker.com/_/redis
  - amazon/dynamodb-local: https://hub.docker.com/r/amazon/dynamodb-local
  - mcr.microsoft.com/mssql/server: https://hub.docker.com/_/microsoft-mssql-server
  - Run the images:
    - mongodb: `docker run --name mongo_server -d mongodb/mongodb-community-server:latest`
    - redis: if you installed Redis via Homebrew, use `redis-server`, otherwise use `docker run --name redis_server -d redis`
    - dynamodb: `docker run --name dynamodb-server -d amazon/dynamodb-local`
    - mssql: `docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=adm1nP@ssword" -e "MSSQL_PID=Express" -p 1433:1433 --name mssql_server -d mcr.microsoft.com/mssql/server:2022-latest`
- Open the project **<span style="color: orange;">Halotsql</span>** and follow the Readme.md there to setup database.

### 3. Notices
There are some important notices that you need to keep in mind while working on the API project:
- Not all the services are registered to be dependency injection, but all the service (abstract) factories are, so you'd better use the service factories instead of the 
  services.
- Magicians are not welcomed here, so don't leave any magics in the codes. Please consider using constants, enums, scope variables, appsetings.json and environment variables if 
  you find your magics may appear.
- **<span style="color: orange;">Important:</span>** please import the IDE settings for your IDE, we have the settings for Rider and Visual Studio in our assets directory.
- **<span style="color: red;">Very important</span>**: the solution should be simple regardless how complex the problem is. Simple yet fully expandable and maintainable is 
  much better than complex and same-ables ones. Decide the level of code complexity *wisely*.