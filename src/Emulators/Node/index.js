const log4js = require("log4js");
const fs = require("fs");

// Connection string - primary key of the Event Hubs namespace
const connectionString = process.env.EH_CONNECTION_STRING;

// Name of the event hub
const eventHubsName = process.env.EH_NAME;

// Main entry point method of the Node Emulator
async function main() {
  let elements = JSON.parse(fs.readFileSync("sample-data-emp.json"));

  log4js.configure({
    appenders: {
      eventhub: {
        type: "@jslogger/eventhub-appender",
        connectionString: connectionString,
        eventHubsName: eventHubsName,
        trigram: "arc",
        application: "My great arc app",
        layer: "lifetest"
      }
    },
    categories: {
      default: { appenders: ["eventhub"], level: "info" }
    }
  });

  const logger = log4js.getLogger();

  for (let i = 0; i < elements.length; i++) {
    logger.info(elements[i].message);
  }
}

// Registering logger around the main() method
main().catch(err => {
  console.log("Error occurred: ", err);
});
