# Structure of the folders

The source code is split this way:

- Components: includes all loging components:
    - Dotnet: the .NET ones
        - [Log4NetAppender](./Components/Dotnet/Log4netAppender): the specific Event Hub appender for Log4Net
        - [NLogTarget](./Components/Dotnet/NlogTarget): the specific Event Hub target for NLog
    - [Java](./Components/Java): the Logback Event Hub appender for Java
    - [Node](./Components/Node): the Azure Event Hub Appender (HTTP) for log4js-node
    - [Python](./Components/Python): a specific Python logger to Even Hub
- [CreateTrigramTable](./CreateTrigramTable): Code to load a table into Azure Storage and used by the Azure Function to validate that a trigram exists
- [Transformation](./Transformation): the core Azure Function, validating, transforming and posting the events into Azure Elastic Cloud
- [Transformation.Tests](./Transformation.Tests): the unit tests associated to the Transformation Azure Function