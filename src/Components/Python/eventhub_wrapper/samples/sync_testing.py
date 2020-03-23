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
            if i % 100 == 0:
                print("{}".format(i))

    print("Send messages in {} seconds.".format(time.time() - start_time))
