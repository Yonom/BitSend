BitSend
=======

BitSend allows you to send information over an unreliable source, reliably!

## About
BitSend was created to send byte arrays using the "c" coin message in everybody edits to other users.  
The protocol is able to handle "missed" coin packets that happen from time to time. This is archived by adding missed packets to the end of the stream. Once a chunk of information has been transmitted, it is parsed and all missed packets are insterted.  
BitSend supports sending byte arrays that take up to 8GB. (As long as you have enough ram and can wait months for it to be transfered!)  
The average sending speed is around 370 bytes / s.
