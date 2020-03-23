from typing import Dict

import time
from azure.eventhub import EventHubProducerClient, EventData



def send_event_data_batch(producer: EventHubProducerClient, data: str):
    """Send/publish data to events hub syncrounously
                Without specifying partition_id or partition_key
                the events will be distributed to available partitions via round-robin.

                Parameters
                ----------
                producer: the eventhub client
                data: data to be published in string format

                Returns
                -------
                none
        """

    event_data_batch = producer.create_batch(partition_id="0")
    event_data_batch.add(EventData(data))
    producer.send_batch(event_data_batch)

def send_event_data_batch_with_partition_key(producer: EventHubProducerClient, data: str, pkey: str):
    """Send/publish data to events hub syncrounously
                Specifying partition_key
                Parameters
                ----------
                producer: the eventhub client
                data: data to be published in string format
                pkey: partion key string

                Returns
                -------
                none
        """
    event_data_batch_with_partition_key = producer.create_batch(partition_key=pkey)
    event_data_batch_with_partition_key.add(EventData(data))
    producer.send_batch(event_data_batch_with_partition_key)


def send_event_data_batch_with_partition_id(producer: EventHubProducerClient, data: str, pid: int):
    """Send/publish data to events hub syncrounously
            Specifying partition_key
            Parameters
            ----------
            producer: the eventhub client
            data: data to be published in string format
            pid: partion key string

            Returns
            -------
            none
    """
    event_data_batch_with_partition_id =  producer.create_batch(partition_id=str(pid))
    event_data_batch_with_partition_id.add(EventData(data))
    producer.send_batch(event_data_batch_with_partition_id)


def send_event_data_batch_with_properties(producer: EventHubProducerClient, data: str, properties: Dict[str, str]):
    """Send/publish data to events hub syncrounously
            Parameters
            ----------
            producer: the eventhub client
            data: data to be published in string format
            properties: a dictionary of key=value of data to be added to the message
            Returns
            -------
            none
    """

    event_data_batch =  producer.create_batch()
    event_data = EventData(data)
    event_data.properties = properties
    event_data_batch.add(event_data)
    producer.send_batch(event_data_batch)


