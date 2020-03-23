# Azure Eventhub python wrapper

This wrapper will publish messages to the azure eventhub.  IT supports both sync and async mode of publishing messages

# To install
```python
pip install eventhub_wrapper
```

or from source:
```python
pip install <path to event_hub directory>

```
 
# Features!
The eventhub configuration is read from a default configuration file named credentials.json.  

 
### Sample credentials.json:
```json
{
	"EVENTSHUB_ACCOUNT_NAME": "<eventhubs account name>",
	"EVENTSHUB_NAME": "<eventshub name>",
	"EVENTSHUB_CONNECTION_STRING":"Endpoint=sb://xxxxxxxxxxxx.servicebus.windows.net/;SharedAccessKeyName=xxxxxxxxxx;SharedAccessKey=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
}
```
### Sample Async Publisher
```python
import time
import asyncio
import eventhub_wrapper.asynchronous.eventhub_async_wrapper as wrapper
import eventhub_wrapper.asynchronous.log_async_publisher as log_async_publisher

'''
        This is a samnple program demonstrating how to publish to events hub using the async wrapper
        the program publishes 1000 messages
        the messages are simple string
         
         Parameters
         ----------
         producer: the eventhub async client

         Returns
         -------
         none
'''

async def run(producer):
    async with producer:
        for i in range(1000):
            await wrapper.send_event_data_batch(producer, "testing {}".format(i))
            if i % 100 == 0:
                print("{}".format(i))


if __name__ == '__main__':

    ### use the utility setup method to create a producer (publisher) object

    producer = log_async_publisher.eventhub_setup()

    ### since the producer is async - an eventloop is needed to ensure are tasks are complete before exiting.

    loop = asyncio.get_event_loop()

    #### performance measure -
    start_time = time.time()

    #### the loop ensures all messages are published.  in the production code, run_forever() may need to be used
    loop.run_until_complete(run(producer))
    print("Send messages in {} seconds.".format(time.time() - start_time))
```

### Sample Sync Publisher
```python
import time

import eventhub_wrapper.synchronous.log_publisher as log_publisher

'''
        This is a samnple program demonstrating how to publish to events hub using the sync wrapper
        the program publishes 1000 messages
        the messages are simple string

         Parameters
         ----------
        none
         Returns
         -------
        none
'''
if __name__ == '__main__':
    # performance measure.
    start_time = time.time()
    # using the utility function, an eventshub object is created in sync mode
    producer = log_publisher.eventhub_setup()
    # printing the events hub properties.  if not connected, this method with throw an exception
    print("event publisher properties are:", producer.get_eventhub_properties())

    # publish a 1000 messages of simple string
    with producer:
        for i in range (1000):
            log_publisher.publish_event(producer, 1, "testing {}".format(i))
            if i % 10 == 0:
                print("{}".format(i))

    print("Send messages in {} seconds.".format(time.time() - start_time))
```