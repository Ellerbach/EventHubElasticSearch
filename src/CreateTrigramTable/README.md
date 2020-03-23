# Create the trigram table with webhook

This program allow to create the trigram table with the webhook. You can use it to add additional information for your own validation.

The file that is importated is a CSV file coma separated and looks like that:

```
NAME,APPLICATION TRIGRAM,WEBHOOK
Super ARC Application,ARC,
```

The first line is ignore during the upload. You can adjust the code to take more columns. You'll need to adjust the [trigram](./Model/trigram.cs) class for any additional element you'll want to import.