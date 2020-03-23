package com.javalogger.emp.logstash;

import ch.qos.logback.classic.Level;
import ch.qos.logback.classic.Logger;
import ch.qos.logback.classic.spi.LoggingEvent;

import java.time.LocalDateTime;

public class EventHubLoggingEvent extends LoggingEvent {

    private String applicationTrigram;

    private String applicationName;

    private String applicationLayer;

    private LocalDateTime date;

    public EventHubLoggingEvent() {
    }

    public EventHubLoggingEvent(String fqcn, Logger logger, Level level, String message, Throwable throwable,
                                Object[] argArray, String applicationTrigram, String applicationName,
                                String applicationLayer, LocalDateTime date) {
        super(fqcn, logger, level, message, throwable, argArray);
        this.applicationTrigram = applicationTrigram;
        this.applicationName = applicationName;
        this.applicationLayer = applicationLayer;
        this.date = date;
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

    public String getApplicationLayer() {
        return applicationLayer;
    }

    public void setApplicationLayer(String applicationLayer) {
        this.applicationLayer = applicationLayer;
    }

    public LocalDateTime getDate() {
        return date;
    }

    public void setDate(LocalDateTime date) {
        this.date = date;
    }
}