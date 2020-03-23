# Log4Net Azure Eventhub Appender

The **Azure Eventhub** appender for **Log4Net** sends *JSON* formatted log events to **Azure Eventhub**. This appender is based on **Log4Net** appender skeleton and uses **Azure Eventhub SDK** to send the events.

## Configuration

This appender will be grabbing the *EventHub Connection String* and the *EventHub Name* from the **System Environment variables**.

The keys format :

* **EH_CONNECTION_STRING**: EventHub Connection String
* **EH_NAME**: EventHub Name