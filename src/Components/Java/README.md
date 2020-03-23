# Logback Event Hubs appender

The Logback Event Hubs appender is a custom Logback appender that brodcasts the Logs from Java/Maven applications to Azure Event Hubs.

The source code/dependencies are built using Java8+ and Maven 3.x

The Emulator needs two parameters:

- **EH_CONNECTION_STRING**: Azure Eventhubs Connection string
- **EH_NAME**: Azure Eventhub Name

The source code can be built using:

```bash
mvn clean install
```

To use this custom Logback Event Hubs appender, just add the Maven dependency to your application:

```xml
<dependency>
    <groupId>com.javalogger.emp</groupId>
    <artifactId>javalogger-logback-eventhub</artifactId>
    <version>0.0.1</version>
</dependency>
```

This Logback configuration in `logback.xml`:

```
<appender name="eventhub" class="com.javalogger.emp.logstash.EventHubAppender">
    <applicationTrigram>arc</applicationTrigram>
    <applicationName>Super name</applicationName>
    <applicationLayer>lifetest</applicationLayer>
</appender>
```
