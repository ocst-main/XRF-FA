# -*- coding: utf-8 -*-
"""
Created on Tue May 31 15:27:07 2022

@author: ThanhOffice
"""

import socket
from threading import Thread
from datetime import datetime


class Server:
    def __init__(self, host, port):
        self.host = host
        self.port = port
        self.server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server.bind((self.host, self.port))
        self.server.listen(5)
        self.sample_id = ""
        self.application = ""
        self.response = ""
        self.listen_for_clients()

    def listen_for_clients(self):
        print(f'{self.host}:{self.port}--> Listening...')
        while True:
            client, addr = self.server.accept()
            print(
                'Accepted Connection from: ' + str(addr[0]) + ':' + str(addr[1])
            )
            Thread(target=self.handle_client, args=(client, addr)).start()

    def handle_client(self, client_socket, address):
        size = 1024
        while True:
            try:
                data = client_socket.recv(size)
                decoded = data.decode()
                now = datetime.now().strftime("%Y.%m.%d %H:%M:%S")
                print(decoded)
                if "@STATUS_REQUEST@SYSTEM@END" in decoded:
                    self.response = f"STATUS\nTIME_STAMP={now}\nSYSTEM=remote\nEND"
                    client_socket.sendall(self.response.encode())
                elif "@STATUS_REQUEST@LIST@END" in decoded:
                    self.response = f"STATUS\nTIME_STAMP={now}\
                    \nLIST\nSTATE=nolist\nEND"
                    client_socket.sendall(self.response.encode())
                elif "@LIST@OPEN@NAME" in decoded:
                    self.response = f"LIST\nSTATUS=normal\nEND"
                    client_socket.sendall(self.response.encode())
                elif "@LIST@START@END" in decoded:
                    self.response = f"LIST\nSTATUS=normal\nEND"
                    client_socket.sendall(self.response.encode())
                elif "@LIST@STOP@END" in decoded:
                    self.response = f"SAMPLE\nREMOVED\nSAMPLE_ID=TEMPB\nMANUAL\nEND"
                    client_socket.sendall(self.response.encode())
                elif "@SAMPLE@ADD@SAMPLE_ID=" in decoded:
                    for item in str(decoded).split("@"):
                        if "SAMPLE_ID" in item:
                            self.sample_id = item.split("=")[-1]
                            break
                    self.response = f"SAMPLE\nADD\
                    \nSAMPLE_ID={self.sample_id}\nSTATUS=normal\nEND"
                    client_socket.sendall(self.response.encode())
                elif "@SAMPLE@REMOVE@SAMPLE_ID=" in decoded:
                    for item in str(decoded).split("@"):
                        if "SAMPLE_ID" in item:
                            self.sample_id = item.split("=")[-1]
                            break
                    self.response = f"SAMPLE\nREMOVE\
                    \nSAMPLE_ID={self.sample_id}\nSTATUS=normal\nEND"
                    client_socket.sendall(self.response.encode())
                elif "MEASMP" in decoded:
                    decoded_split = str(decoded).split('"')
                    self.sample_id = decoded_split[1]
                    self.application = decoded_split[3]
                    self.response = f"SAMPLE\nMEASURED\nSAMPLE_ID={self.sample_id}\nSTATUS=normal\nEND"
                    client_socket.sendall(self.response.encode())
                    self.response = f"RESULT\nSAMPLE_ID={self.sample_id}\nSTATUS=result_ok\nTIME_STAMP={now}\nAPPLICATION={self.application}\nNORMFACTOR=1\nINITIAL_WEIGHT=1\nFINAL_WEIGHT=1\nCHAN=Fe2\nCOMP=Fe\nCONC=0\nINT=1920.6\nCONCUNIT=g/m2\nCHAN=Fe2\nCOMP=Al*\nCONC=0\nINT=1920.6\nCONCUNIT=g/m2\nCHAN=Fe2\nCOMP=Zn\nCONC=37.25944\nINT=1920.6\nCONCUNIT=g/m2\nCHAN=Cr1\nCOMP=Cr1\nCONC=0\nINT=3.0242\nCONCUNIT=kcps\nEND"
                    client_socket.sendall(self.response.encode())
                elif "READRS" in decoded:
                    decoded_split = str(decoded).split('"')
                    self.sample_id = decoded_split[1]
                    self.response = f"SAMPLE\nMEASURED\nSAMPLE_ID={self.sample_id}\nSTATUS=normal\nEND"
                    client_socket.sendall(self.response.encode())
                    self.response = f"RESULT\nSAMPLE_ID={self.sample_id}\nSTATUS=result_ok\nTIME_STAMP={now}\nAPPLICATION={self.application}\nNORMFACTOR=1\nINITIAL_WEIGHT=1\nFINAL_WEIGHT=1\nCHAN=Fe2\nCOMP=Fe\nCONC=0\nINT=1693.984\nCONCUNIT=g/m2\nCHAN=Fe2\nCOMP=Al*\nCONC=0\nINT=1693.984\nCONCUNIT=g/m2\nCHAN=Fe2\nCOMP=Zn\nCONC=44.35837\nINT=1693.984\nCONCUNIT=g/m2\nCHAN=Cr1\nCOMP=Cr1\nCONC=0\nINT=2.6393\nCONCUNIT=kcps\nEND"
                    client_socket.sendall(self.response.encode())
                elif "@LIST@STOP@END" in decoded:
                    self.response = f"SAMPLE\nREMOVED\nSAMPLE_ID=TEMPB\nMANUAL\nEND"
                    client_socket.sendall(self.response.encode())
                print(self.response)

            except socket.error:
                client_socket.close()


if __name__ == "__main__":
    host = '127.0.0.1'
    port = 1904
    main = Server(host, port)
    # start listening for clients
    main.listen_for_clients()