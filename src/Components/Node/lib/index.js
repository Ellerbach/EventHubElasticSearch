"use strict";

/**
 * Azure Event Hubs appender sends JSON formatted log events to an Azure Event Hubs.
 */
const eventhub = require("@azure/event-hubs");

function log4jsEventHubAppender(config) {
  const sender = eventhub.EventHubClient.createFromConnectionString(
    config.connectionString,
    config.eventHubsName
  );

  return async function log(event) {
    const date = new Date();

    event.message = event.data[0];
    event.trigram = config.trigram;
    event.application = config.application;
    event.layer = config.layer;
    event.date = date.toISOString();

    //
    // send to server
    //
    await sender.send(event).catch(error => {
      if (error.response) {
        console.error(
          `log4js.eventhub Appender error posting to ${config.url}: ${error.response.status} - ${error.response.data}`
        );
        return;
      }
      console.error(`log4js.eventhub Appender error: ${error.message}`);
    });

    await sender.close();
  };
}

function configure(config) {
  return log4jsEventHubAppender(config);
}

module.exports.configure = configure;
