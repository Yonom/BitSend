# BitSend Protocol

The BitSend protocol allows you to send information over an unreliable source, reliably!

# Chunks

A chunk is a byte array sent over the BitSend protocol. Chunks can be as big as around 8GB, however, it is not recommended to send really big chunks, as they limit the amount of error correction that can be performed.

# Packets

Messages are sent in packets. There are 6 types of packets. Each packet has 4 bytes.

## Data packets

Packets that contain data must start with their first bit set. They contain max. 30 bits of data. All bits after the first unset bit are data. It is invalid to send a data message with less than 1 bit of data.

```
10XXXXXX XXXXXXXX XXXXXXXX XXXXXXXX
110XXXXX XXXXXXXX XXXXXXXX XXXXXXXX
1110XXXX XXXXXXXX XXXXXXXX XXXXXXXX
.....
11111111 11111111 11111111 111110XX
11111111 11111111 11111111 1111110X
```

X: Data.

## Repair packets

If a packet goes missing, a repair packet pair can be sent. The first packet will contain the message data.
The second packet will be an int (max value (2 ^ 31) - 33) determining at which position the packet must be inserted.
Once a repair message has been sent, the packet counter must be subtracted by 1.
Repair messages must be sent in ascending order.

## Start chunk

A start chunk message has a value of -1. It should be sent before starting to send the message data.

```
111111111 11111111 11111111 11111111
```


## End chunk

A end chunk message has a value of -2.

```
111111111 11111111 11111111 11111110
```

## Hai packets

Once a BitSend compatible clients connects, it should send a hai packet. The hai packet has a value of (2^31)-1 = 2,147,483,647.

```
011111111 11111111 11111111 11111111
```

## Hey packets

If a client receives a hai packet. It should respond with a hey packet. The hey packet has a value of (2^31)-2 = 2,147,483,646.

```
011111111 11111111 11111111 11111110
```

## Bai packets

If a BitSend client no longer wishes to use the protocol, it should send a bai packet. It has a value of (2^31)-3 = 2,147,483,645.

```
011111111 11111111 11111111 11111101
```

# Processing chunks
Before the sender sends a break chunk message, it must make sure that all packets have been sent sucessfully or have been repaired. Once the receiver receives the break chunk message, it can start parsing it.
There are two steps in this process:
First, packets are analyzed backwards. Once the client hits a repair message. It will insert that message in the position specified. 
Once all packets have been repaired, the receiver adds all received bits together to generate a byte array.
