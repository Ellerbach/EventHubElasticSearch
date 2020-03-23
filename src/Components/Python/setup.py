from setuptools import setup, find_packages
from distutils.core import setup
import os

setup(
  name = 'eventhub-wrapper',
  package_dir={'': '.'},
  packages = find_packages(exclude=["samples"]),   ##['eventhub-wrapper'],Chose the same as "name"
  version = os.getenv('GITVERSION_SEMVER'),
  license='MIT',
  description = 'The eventhub wrapper to publish messges sync or async',
  author = 'Art Sedighi',
  author_email = 'asedighi@microsoft.com',
  url = 'https://github.com/asedighi/eventhub-python',
  download_url = 'https://github.com/asedighi/eventhub-python/archive/v_01.tar.gz',
  keywords = ['azure', 'eventshub', 'python', 'wrapper'],
  setup_requires=[
    'azure-eventhub==5.0.0',
    'asyncio',
      ],
  install_requires=[
          'azure-eventhub==5.0.0',
          'asyncio',
      ],
  classifiers=[
    'Development Status :: 3 - Alpha',
    'Intended Audience :: Developers',
    'Topic :: Software Development :: Build Tools',
    'License :: OSI Approved :: MIT License',
    'Programming Language :: Python :: 3.7',
  ],
  include_package_data=True,

)