import time
from azure.eventhub import EventHubProducerClient

from eventhub_wrapper.config import AzureCredentials
import eventhub_wrapper.synchronous.eventhub_wrapper as wrapper

### These four ASYNC types are currently supported
### a simple SYNC publisher is also supported

TYPE_SYNC_SIMPLE = 0
TYPE_SYNC_WITH_PARTITION_KEY = 1
TYPE_SYNC_WITH_PARTITION_ID = 2
TYPE_SYNC_WITH_PROPERTIES = 3



def eventhub_setup():
    """ this method read the config file and creates an eventshub client in sync fashion.

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


def publish_event(publisher: EventHubProducerClient, publisher_type: int, data: str, *argv):
    """ this method publishes the message using on the methods supported by the wrapper

                Parameters
                ----------
               publisher_type: 0: simple sync publish
                                1: sync publish with partition key
                                2: sync publish with partition id
                                3: sync publish with a dict of key=value
                                default: simple sync publish
                Returns
                -------
                none
    """

    if publisher_type == 0:
        wrapper.send_event_data_batch(publisher, data)
    elif publisher_type == 1:
        wrapper.send_event_data_batch_with_partition_key(publisher, data, argv)
    elif publisher_type == 2:
        wrapper.send_event_data_batch_with_partition_id(publisher, data, int(argv))
    elif publisher_type == 3:
        wrapper.send_event_data_batch_with_properties(publisher, data, int(argv))
    else:
        wrapper.send_event_data_batch(publisher, data)




