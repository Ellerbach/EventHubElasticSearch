from azure.eventhub.aio import EventHubProducerClient

from eventhub_wrapper.config import AzureCredentials
import eventhub_wrapper.asynchronous.eventhub_async_wrapper as wrapper


### These four ASYNC types are currently supported
### a simple SYNC publisher is also supported
TYPE_ASYNC_SIMPLE = 0
TYPE_ASYNC_WITH_PARTITION_KEY = 1
TYPE_ASYNC_WITH_PARTITION_ID = 2
TYPE_ASYNC_WITH_PROPERTIES = 3



def eventhub_setup():
    """ this method read the config file and creates an eventshub client in async fashion.

                Parameters
                ----------
               none
                Returns
                -------
                the events hub publisher object needed to publish events

        """

    _configuration = AzureCredentials()
    _eventhub_name = _configuration.get_eventhub_name()
    _conn_str = _configuration.get_eventhub_conn_string()


    print("connection string is: ", _conn_str)
    print("event hub name is: ", _eventhub_name)
    _publisher = EventHubProducerClient.from_connection_string(conn_str=_conn_str, eventhub_name=_eventhub_name)
    return _publisher


async def publish_event(publisher: EventHubProducerClient, publisher_type: int, data: str, *argv):
    """ this method publishes the message using on the methods supported by the wrapper

                Parameters
                ----------
               publisher_type: 0: simple async publish
                                1: async publish with partition key
                                2: async publish with partition id
                                3: async publish with a dict of key=value
                                default: simple async publish
                Returns
                -------
                none
    """

    if publisher_type == 0:
        await wrapper.send_event_data_batch(publisher, data)
    elif publisher_type == 1:
        await wrapper.send_event_data_batch_with_partition_key(publisher, data, argv)
    elif publisher_type == 2:
        await wrapper.send_event_data_batch_with_partition_id(publisher, data, int(argv))
    elif publisher_type == 3:
        await wrapper.send_event_data_batch_with_properties(publisher, data, int(argv))
    else:
        await wrapper.send_event_data_batch(publisher, data)




