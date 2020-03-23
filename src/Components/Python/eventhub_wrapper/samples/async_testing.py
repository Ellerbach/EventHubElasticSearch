import time
import asyncio

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


import eventhub_wrapper.asynchronous.eventhub_async_wrapper as wrapper
import eventhub_wrapper.asynchronous.log_async_publisher as log_async_publisher


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

