# AgentZero
A client-server tool for remote LLMNR/NBTNS poisoning by Matt Kiely (HuskyHacks) and Aaron Hobdy.

## Roadmap
- [x] Python socket server that accepts input and writes to log file
- [x] InveighZero exe that sends captured NTLMv2 hash to server
- [x] Flask App
- [x] Stores in database
- [ ] Dynamically compile InveighZero with IP address and port
- [ ] Multi-threaded, multi-agent handling
- [ ] Front end web client for multi-agent config/administration
- [ ] Dynamic generation for listener URL
- [ ] Encoding/encrypting for exfil
  - [ ] Base64 encoding
  - [ ] Asymmetrical Key exchange at compilation/agent check in
