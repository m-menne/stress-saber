# Minimalistic implementation of a client for receiving the heart rate values send by the server.

import socket

HOST = "192.168.1.84"
PORT = 1111

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST,PORT))
    data = s.recv(1024)
    print(data.decode('utf-8'))
    while True:
        data = s.recv(1024)
        print("Heart Rate: " + str(int.from_bytes(data, "big")))
