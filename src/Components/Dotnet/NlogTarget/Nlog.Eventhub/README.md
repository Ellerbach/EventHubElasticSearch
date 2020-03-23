# NLog logging for Event Hub

This class will post to **Azure Event Hubs** with standard NLog login. 

## Configuration

This appender will be grabbing the *EventHub Connection String*, the *Event Hub Name* and the *Event Hub transport type* from the **System Environment variables**.

The keys format :

* **NL_CONNECTION_STRING**: Event Hub Connection String
* **NL_NAME**: Event Hub Name
* **NL_TRANSPORT_TYPE**: Event Hub trnasport type