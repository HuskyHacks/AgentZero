# AgentZero
A client-server tool for remote LLMNR/NBTNS poisoning by Matt Kiely (HuskyHacks) and Aaron Hobdy.

## Roadmap
- [x] Python socket server that accepts input and writes to log file
- [x] InveighZero exe that sends captured NTLMv2 hash to server
- [x] Flask App
- [ ] Stores in database
- [ ] Dynamically compile InveighZero with IP address and port
- [ ] Multi-threaded, accepts any data from InveighZero and parses
- [ ] Front end web client for multi-agent config/administration