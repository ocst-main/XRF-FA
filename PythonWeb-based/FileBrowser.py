# -*- coding: utf-8 -*-
"""
Created on Wed Jun  8 22:43:39 2022

@author: ThanhHome
"""

from flask import Flask 
from flask_autoindex import AutoIndex
from waitress import serve

import configparser
import os

config = configparser.ConfigParser()

config.read(r'Config\SQLite.ini')
folder_directory = config.get("CONFIG","DIRECTORY")
if not os.path.exists(folder_directory):
    os.mkdir(folder_directory)

app = Flask(__name__)
AutoIndex(app, browse_root=folder_directory)

if __name__ == "__main__":
    try:
        config.read(r'Config\WebPort.ini')
        port = config.get("CONFIG","PORT")
        port = int(port)
    except:
        port = 30080
    serve(app, host='0.0.0.0', port=port)

    