# This script implements the functionality of the heart rate server.
# A TCP socket is opend and a connection to the client is established. After setting the notifatication flag for the heart rate value handler, heart rate values are received and stored.
# Furthermore, the last received heart rate value is send to the client via the TCP connection.

import pexpect
import sys
import socket
import time
import errno

HW_ADDRESS = "ED:27:9F:68:8B:03"    # Hardware address of the bluetooth heart rate monitor.
ATTR_HANDLE = "0x001a"              # Attribute handler of the heart rate value.

HOST = ""       # IP address of the server.
PORT = 1111     # Port number of the socket of the server.

heart_rate_values = []  # Stores all received heart rate values.

connection_alive = False    # Flag which indicates whether the connection to the client is alive.

# Initalize a TCP socket.
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))    # Bind the socket to the given IP address and port.
    s.listen()              # Start listening.
    while True:
        print("Waiting for client...")
        connection, addr = s.accept()
        # Connection to client is established.
        with connection:
            print("Connected to " + str(addr[0]))
            connection_alive = True
            while connection_alive:
                while connection_alive:
                    # Set notification flag for the heart rate value handler by using the gatttool.
                    child = pexpect.spawn('sudo gatttool -t random -b ' + HW_ADDRESS + ' --char-write-req -a ' + ATTR_HANDLE + ' -n 0100 --listen', encoding='utf-8')
                    # Wait for positive acknowledgement regarding the notification flag being set.
                    try:
                        child.expect('Characteristic value was written successfully', timeout = 5)
                    except pexpect.TIMEOUT:
                        print("ERROR: Characteristic value could not be written!")
                        child.close(force=True)
                        break
                    except pexpect.EOF:
                        error_message = child.before
                        print("ERROR: " + error_message)
                        child.close(force=True)
                        break
                    else:
                        while connection_alive:
                            # Receive heart rate values from the heart rate monitor.
                            try:
                                child.expect('Notification handle = 0x0019 value: 00 ', timeout = 2)
                            except pexpect.TIMEOUT:
                                #print("Warning: Lost connection to heart rate sensor. Reconnecting...")
                                child.close(force=True)
                                break
                            else:
                                child.expect(' \r\n', timeout = 2)
                                heart_rate_values.append(int(child.before,16))          # Store received heart rate values.
                                try:
                                    connection.send(bytes([heart_rate_values[-1]]))     # Send last received heart rate value to client.
                                except IOError as e:
                                    if e.errno == errno.EPIPE:                          # Check if client has closed the connection.
                                        print("Client closed connection...")
                                        child.close(force=True)
                                        connection_alive = False
                                else:
                                    print("Heart Rate: " + str(heart_rate_values[-1]))
