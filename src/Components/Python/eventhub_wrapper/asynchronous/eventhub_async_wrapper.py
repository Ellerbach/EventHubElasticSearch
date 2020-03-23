from typing import Dict

import time
from azure.eventhub.aio import EventHubProducerClient
from azure.eventhub import EventData


async def send_event_data_batch(producer: EventHubProducerClient, data: str):
    """Send/publish data to events hub asyncrounously
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
    event_data_batch = await producer.create_batch()
    event_data_batch.add(EventData(data))
    await producer.send_batch(event_data_batch)

async def send_event_data_batch_with_partition_key(producer: EventHubProducerClient, data: str, pkey: str):
    """Send/publish data to events hub asyncrounously
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
    event_data_batch_with_partition_key = await producer.create_batch(partition_key=pkey)
    event_data_batch_with_partition_key.add(EventData(data))
    await producer.send_batch(event_data_batch_with_partition_key)


async def send_event_data_batch_with_partition_id(producer: EventHubProducerClient, data: str, pid: int):
    """Send/publish data to events hub asyncrounously
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
    event_data_batch_with_partition_id =  await producer.create_batch(partition_id=str(pid))
    event_data_batch_with_partition_id.add(EventData(data))
    await producer.send_batch(event_data_batch_with_partition_id)


async def send_event_data_batch_with_properties(producer: EventHubProducerClient, data: str, properties: Dict[str, str]):
    """Send/publish data to events hub asyncrounously
            Parameters
            ----------
            producer: the eventhub client
            data: data to be published in string format
            properties: a dictionary of key=value of data to be added to the message
            Returns
            -------
            none
    """
    event_data_batch =  await producer.create_batch()
    event_data = EventData(data)
    event_data.properties = properties
    event_data_batch.add(event_data)
    await producer.send_batch(event_data_batch)


