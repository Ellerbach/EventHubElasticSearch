package com.javalogger.emp.logstash;

import ch.qos.logback.classic.spi.ILoggingEvent;
import ch.qos.logback.core.AppenderBase;
import com.google.gson.Gson;
import com.microsoft.azure.eventhubs.EventData;
import com.microsoft.azure.eventhubs.EventHubClient;
import com.microsoft.azure.eventhubs.EventHubException;

import java.io.IOException;
import java.nio.charset.Charset;
import java.time.Instant;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.util.concurrent.Executors;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * @author nebrass
 */
public class EventHubAppender extends AppenderBase<ILoggingEvent> {

    public static final String DATE_PATTERN = "yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'";

    private String applicationLayer;
    private String applicationTrigram;
    private String applicationName;

    private EventHubClient eventHubClient = null;
    private DateTimeFormatter formatter;
    private final String connectionString = System.getenv("EH_CONNECTION_STRING");
    private final String eventHubsName = System.getenv("EH_NAME");

    public EventHubAppender() {
        this.formatter =
                DateTimeFormatter.ofPattern(DATE_PATTERN);

        try {
            this.eventHubClient = EventHubClient.createFromConnectionStringSync(
                    connectionString + ";EntityPath=" + eventHubsName,
                    Executors.newScheduledThreadPool(4));
        } catch (EventHubException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }

    }

    @Override
    protected void append(ILoggingEvent eventObject) {
        LocalDateTime dateTime = LocalDateTime.ofInstant(
                Instant.ofEpochMilli(eventObject.getTimeStamp()),
                ZoneId.systemDefault()
        );

        String jsonPayload = createJsonObject(
                applicationTrigram, applicationName,
                applicationLayer, eventObject.getLevel().toString(), formatter.format(dateTime), eventObject.getFormattedMessage()
        );

        EventData sendEvent = EventData.create(jsonPayload.getBytes(Charset.defaultCharset()));

        try {
            this.eventHubClient.sendSync(sendEvent);
        } catch (EventHubException ex) {
            Logger.getLogger(EventHubAppender.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    private String createJsonObject(String trigram, String application, String layer, String level, String date, String message) {
        Gson gson = new Gson();

        EventHubMessage eventHubMessage = new EventHubMessage(
                trigram, application, layer, level, date, message
        );

        return gson.toJson(eventHubMessage);
    }

    public String getApplicationLayer() {
        return applicationLayer;
    }

    public void setApplicationLayer(String applicationLayer) {
        this.applicationLayer = applicationLayer;
    }

    public String getApplicationTrigram() {
        return applicationTrigram;
    }

    public void setApplicationTrigram(String applicationTrigram) {
        this.applicationTrigram = applicationTrigram;
    }

    public String getApplicationName() {
        return applicationName;
    }

    public void setApplicationName(String applicationName) {
        this.applicationName = applicationName;
    }
}
