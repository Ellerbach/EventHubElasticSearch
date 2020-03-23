package com.javalogger.emp.logstash;

public class EventHubMessage {

    private final String trigram;
    private final String application;
    private final String layer;
    private final String level;
    private final String date;
    private final String message;

    public EventHubMessage(String trigram, String application, String layer, String level, String date, String message) {
        this.trigram = trigram;
        this.application = application;
        this.layer = layer;
        this.level = level;
        this.date = date;
        this.message = message;
    }

    public String getTrigram() {
        return trigram;
    }

    public String getApplication() {
        return application;
    }

    public String getLayer() {
        return layer;
    }

    public String getLevel() {
        return level;
    }

    public String getDate() {
        return date;
    }

    public String getMessage() {
        return message;
    }
}
