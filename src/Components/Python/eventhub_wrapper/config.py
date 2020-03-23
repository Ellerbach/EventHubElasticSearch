import json
import os

def _find_file_path(name, path="../../.."):
    """ this is a utility method.  it resembles: find . -print | grep name

                    Parameters
                    ----------
                   name: the file name we are looking
                   path: the relative path from current location
                        default: three levels up from current working directory
                    Returns
                    -------
                    the full path of the file or none if nothing is found
            """

    for root, dirs, files  in os.walk(path):
        ####print("{} {} {}".format(root, dirs, files))
        if name in files:
            return os.path.join(root, name)

    return None


class ReadConfig():
    """ this class reads the config file which is in json format
        it tries to find the file using the utility funtion _find_file_path

        Parameters
        ----------
       none
        Returns
        -------
        json sting

    """

    def __init__(self, name=''):
        config = name

        json_config = ''

        if os.path.isfile(config):
            print("Found {}".format(config))
            with open(config) as json_data:
                json_config = json.load(json_data)

        else:
            my_path = _find_file_path(name, "../../../../")
            print("found: {}".format(my_path))
            if os.path.isfile(my_path):
                with open(my_path) as json_data:
                    json_config = json.load(json_data)

        return  json_config

class AzureCredentials():
    """ this class reads provides azure credentials part of the config file to the user
            the default config file is named credentials.json

            Parameters
            ----------
           none
            Returns
            -------
            different parameters are returned as needed
                get_eventhub_account_name: the eventshub account name
                get_eventhub_name: the events hub name
                get_eventhub_conn_string: connection string to connect to the events hub

        """

    def __init__(self):
        '''
        Constructor
        '''
        self.cred = ''

        if os.path.isfile('./credentials.json'):
            print("Found credentials.json in current directory")
            with open('./credentials.json') as json_data:
                self.cred = json.load(json_data)

        else:
            my_path = _find_file_path("credentials.json", "../")
            if my_path == None:
                print("I was not able to find the credentials.json file... exiting....")
                exit(-1)
            print("found: {}".format(my_path))
            if os.path.isfile(my_path):
                with open(my_path) as json_data:
                    self.cred = json.load(json_data)

        '''
        application or client id are the same
        '''
        self.account_name = self.cred['EVENTSHUB_ACCOUNT_NAME']
        self.eventhub_name = self.cred['EVENTSHUB_NAME']
        self.connection_string = self.cred['EVENTSHUB_CONNECTION_STRING']


    def get_eventhub_account_name(self):
        return self.account_name
    
    def get_eventhub_name(self):
        return self.eventhub_name
    
    def get_eventhub_conn_string(self):
        return self.connection_string
    





