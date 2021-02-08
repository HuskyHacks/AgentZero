# AgentZero
A client-server tool for remote LLMNR/NBTNS poisoning by Matt Kiely (HuskyHacks) and Aaron Hobdy.

## Roadmap
- [ ] Python socket server that accepts input and writes to log file
- [ ] InveighZero exe that sends captured NTLMv2 hash to server
- [ ] Flask App that starts server and can dynamically compile InveighZero with IP address and port
- [ ] Multi-threaded, accepts any data from InveighZero and parses
- [ ] Stores in database