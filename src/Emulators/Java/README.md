# Eventhub Sample Logs Emulator

### Presentation
The Eventhub Sample Logs Emulator is a Java application based on the Spring Boot framework. 
This emulator is used to send sample records to the Azure Eventhubs for simulation purposes.

The source code/dependencies are built using Java8+ and Maven 3.x

The Emulator needs two parameters:
* **EH_CONNECTION_STRING**: Azure Eventhubs Connection string
* **EH_NAME**: Azure Eventhub Name

### Steps
The source code can be built using:
```bash
mvn clean install
```

To run the source code:
```bash
mvn spring-boot:run
```

To send the samples JSON objects:
```bash
curl http://localhost:9090/api/doit
```