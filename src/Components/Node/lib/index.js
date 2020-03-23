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

  return function log(event) {
    const date = new Date();

    event.data.trigram = config.trigram;
    event.data.application = config.application;
    event.data.layer = config.layer;
    event.data.date = date.toISOString();
    //
    // send to server
    //
    sender.send(event.data).catch(error => {
      if (error.response) {
        console.error(
          `log4js.eventhub Appender error posting to ${config.url}: ${error.response.status} - ${error.response.data}`
        );
        return;
      }
      console.error(`log4js.eventhub Appender error: ${error.message}`);
    });
  };
}

function configure(config) {
  return log4jsEventHubAppender(config);
}

module.exports.configure = configure;
