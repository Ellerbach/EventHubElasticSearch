package com.microsoft.cse.eventhubemulator;

import lombok.extern.slf4j.Slf4j;
import net.minidev.json.JSONArray;
import net.minidev.json.parser.JSONParser;
import net.minidev.json.parser.ParseException;
import org.springframework.util.ResourceUtils;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;

@Slf4j
@RestController
@RequestMapping("/api")
public class SendSamplesRestController {

    @GetMapping("/doit")
    public String sayHello() throws FileNotFoundException, ParseException {
        JSONParser parser = new JSONParser(JSONParser.MODE_JSON_SIMPLE);

        File sampleJson = ResourceUtils.getFile("classpath:data/sample-data-emp.json");

        JSONArray elements = (JSONArray) parser.parse(new FileReader(sampleJson));

        for (Object a : elements) {
            log.debug(a.toString());
        }
        return String.format("%d JSON Records were logged successfully.", elements.size());
    }
}
