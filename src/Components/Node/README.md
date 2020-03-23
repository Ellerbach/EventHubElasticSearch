# Azure Eventhub Appender (HTTP) for log4js-node

The Azure Eventhub appender for [log4js](https://log4js-node.github.io/log4js-node) send JSON formatted log events to [Azure Eventhub](https://azure.microsoft.com/fr-fr/services/event-hubs/). This appender uses Azure Eventhub SDK to send the events.

```bash
npm install log4js @jslogger/eventhub-appender
```

## Configuration

* `type` - `@jslogger/eventhub-appender`
* `connectionString` - `string` - Azure Event Hub Namespace connection string
* `eventHubsName` - `string` - Azure Event Hub Name

This appender will also pick up Logger context values from the events, and add them as `p_` values in the logFaces event. See the example below for more details.

# Example (default config)

```javascript
log4js.configure({
  appenders: {
    eventhub: { type: '@jslogger/eventhub-appender', connectionString: connectionString, eventHubsName: eventHubsName}
  },
  categories: {
    default: { appenders: [ 'eventhub' ], level: 'info' }
  }
});

const logger = log4js.getLogger();
logger.info(validDataMessage);
```
