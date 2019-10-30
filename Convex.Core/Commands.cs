#region

using System.Collections.Generic;
using System.Linq;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

#endregion

namespace Convex.Core
{
    public static class Commands
    {
        #region MEMBERS

        public const string ALL = "";
        public const string ABORT = "ABORT";
        public const string USER = "USER";
        public const string NICK = "NICK";
        public const string QUIT = "QUIT";
        public const string JOIN = "JOIN";
        public const string PART = "PART";
        public const string PRIVMSG = "PRIVMSG";
        public const string MODE = "MODE";
        public const string TOPIC = "TOPIC";
        public const string KICK = "KICK";
        public const string NOTICE = "NOTICE";
        public const string NAMES = "NAMES";
        public const string LIST = "LIST";
        public const string MOTD = "MOTD";
        public const string VERSION = "VERSION";
        public const string STATS = "STATS";
        public const string LINKS = "LINKS";
        public const string TIME = "TIME";
        public const string CONNECT = "CONNECT";
        public const string ADMIN = "ADMIN";
        public const string INFO = "INFO";
        public const string SERVLIST = "SERVERLIST";
        public const string WHO = "WHO";
        public const string WHOIS = "WHOIS";
        public const string WHOWAS = "WHOWAS";
        public const string KILL = "KILL";
        public const string PING = "PING";
        public const string PONG = "PONG";
        public const string ERROR = "ERROR";
        public const string AWAY = "AWAY";
        public const string REHASH = "REHASH";
        public const string USERS = "USERS";
        public const string USERHOST = "USERHOST";
        public const string ISON = "ISON";
        public const string LUSERS = "LUSERS";
        public const string NJOIN = "NJOIN";

        /// <summary>
        ///     [nick]![user]@[host]
        /// </summary>
        public const string RPL_WELCOME = "001";

        /// <summary>
        ///     Host is [servername]; version [ver]
        /// </summary>
        public const string RPL_YOURHOST = "002";

        /// <summary>
        ///     Created [date]
        /// </summary>
        public const string RPL_CREATIONDATE = "003";

        /// <summary>
        ///     [servername] [version] [available user modes] [available channel modes]
        /// </summary>
        public const string RPL_YOURINFO = "004";

        /// <summary>
        ///     Try server [server name]; port [port number]
        /// </summary>
        public const string RPL_ALTSERVER = "005";

        /// <summary>
        ///     [linkname] [sendq] [sent messages] [sent Kbytes] [received messages] [received Kbytes] [time open]
        /// </summary>
        public const string RPL_STATSLINKINFO = "211";

        /// <summary>
        ///     [command] [count] [byte count] [remote count]
        /// </summary>
        public const string RPL_STATSCOMMAND = "212";

        /// <summary>
        ///     [stats letter] :End of STATS report
        /// </summary>
        public const string RPL_STATSEND = "219";

        /// <summary>
        ///     [user mode const string]
        /// </summary>
        public const string RPL_UMODEIS = "221";

        /// <summary>
        ///     [name] [server] [mask] [type] [hopcount] [info]
        /// </summary>
        public const string RPL_SERVERLIST = "234";

        /// <summary>
        ///     [mask] [type] :End of service listing
        /// </summary>
        public const string RPL_SERVERLISTEND = "235";

        /// <summary>
        ///     :Server Up %d days %d:%02d:%02d
        /// </summary>
        public const string RPL_STATSUPTIME = "242";

        /// <summary>
        ///     O [hostmask] * [name]
        /// </summary>
        public const string RPL_STATSONLINE = "243";

        /// <summary>
        ///     :There are [integer] users and [integer] services on [integer] servers
        /// </summary>
        public const string RPL_LUSERCLIENT = "251";

        /// <summary>
        ///     [integer] :operator(s) online
        /// </summary>
        public const string RPL_LUSEROP = "252";

        /// <summary>
        ///     [integer] :unknown connection(s)
        /// </summary>
        public const string RPL_LUSERUNKNOWN = "253";

        /// <summary>
        ///     [integer] :channels formed
        /// </summary>
        public const string RPL_LUSERCHANNELS = "254";

        /// <summary>
        ///     :I have [integer] clients and [integer] servers
        /// </summary>
        public const string RPL_LUSERME = "255";

        /// <summary>
        ///     [server] :Administrative info
        /// </summary>
        public const string RPL_ADMINME = "256";

        /// <summary>
        ///     :[admin info]
        /// </summary>
        public const string RPL_ADMINLOC1 = "257";

        /// <summary>
        ///     :[admin info]
        /// </summary>
        public const string RPL_ADMINLOC2 = "258";

        /// <summary>
        ///     :[admin info]
        /// </summary>
        public const string RPL_ADMINEMAIL = "259";

        /// <summary>
        ///     File [logfile] [debuglevel]
        /// </summary>
        public const string RPL_TRACELOG = "261";

        /// <summary>
        ///     [servername] [version & debuglevel] :End of TRACE
        /// </summary>
        public const string RPL_TRACEEND = "262";

        /// <summary>
        ///     [command] :Please wait a while and try again.
        /// </summary>
        public const string RPL_TRYAGAIN = "263";

        /// <summary>
        ///     [nick] :[away message]
        /// </summary>
        public const string RPL_AWAY = "301";

        /// <summary>
        ///     :*1[reply] *( " " [reply] )
        /// </summary>
        public const string RPL_USERHOST = "302";

        /// <summary>
        ///     :*1[nick] *( " " [nick] )
        /// </summary>
        public const string RPL_ISON = "303";

        /// <summary>
        ///     :No longer marked away
        /// </summary>
        public const string RPL_UNAWAY = "305";

        /// <summary>
        ///     :You have been marked as away
        /// </summary>
        public const string RPL_NOWAWAY = "306";

        /// <summary>
        ///     [nick] [user] [host] * :[real name]
        /// </summary>
        public const string RPL_WHOISUSER = "311";

        /// <summary>
        ///     [nick] [server] :[server info]
        /// </summary>
        public const string RPL_WHOISSERVER = "312";

        /// <summary>
        ///     [nick] :is an IRC operator
        /// </summary>
        public const string RPL_WHOISOPERATOR = "313";

        /// <summary>
        ///     [nick] [user] [host] * :[real name]
        /// </summary>
        public const string RPL_WHOWASUSER = "314";

        /// <summary>
        ///     [nick] :End of WHOWAS
        /// </summary>
        public const string RPL_ENDOFWHO = "315";

        /// <summary>
        ///     [nick] [integer] :seconds idle
        /// </summary>
        public const string RPL_WHOISIDLE = "317";

        /// <summary>
        ///     [nick] :End of WHOIS list
        /// </summary>
        public const string RPL_ENDOFWHOIS = "318";

        /// <summary>
        ///     [nick] :*( ( "@" / "+" ) [channel] " " )
        /// </summary>
        public const string RPL_WHOISCHANNELS = "319";

        /// <summary>
        ///     [channel] [# visible] :[topic]
        /// </summary>
        public const string RPL_LIST = "322";

        /// <summary>
        ///     :End of LIST
        /// </summary>
        public const string RPL_LISTEND = "323";

        /// <summary>
        ///     [channel] [mode] [mode params]
        /// </summary>
        public const string RPL_CHANNELMODEIS = "324";

        /// <summary>
        ///     [channel] [nickname]
        /// </summary>
        public const string RPL_UNIQOPIS = "325";

        /// <summary>
        ///     [channel] :No topic is set
        /// </summary>
        public const string RPL_NOTOPIC = "331";

        /// <summary>
        ///     [channel] :[topic]
        /// </summary>
        public const string RPL_TOPIC = "332";

        /// <summary>
        ///     [channel] [nick]
        /// </summary>
        public const string RPL_INVITING = "341";

        /// <summary>
        ///     [user] :Summoning user to IRC
        /// </summary>
        public const string RPL_SUMMONING = "342";

        /// <summary>
        ///     [channel] [invitemask]
        /// </summary>
        public const string RPL_INVITELIST = "346";

        /// <summary>
        ///     [channel] :End of channel invite list
        /// </summary>
        public const string RPL_ENDOFINVITELIST = "347";

        /// <summary>
        ///     [channel] [exceptionmask]
        /// </summary>
        public const string RPL_EXCEPTLIST = "348";

        /// <summary>
        ///     [channel] :End of channel exception list
        /// </summary>
        public const string RPL_ENDOFEXCEPTLIST = "349";

        /// <summary>
        ///     [version].[debuglevel] [server] :[comments]
        /// </summary>
        public const string RPL_VERSION = "351";

        /// <summary>
        ///     [channel] [user] [host] [server] [nick] ( "H" / "G" ] ["*"] [ ("@" / "+")] :[hopcount] [real name]
        /// </summary>
        public const string RPL_WHOREPLY = "352";

        /// <summary>
        ///     ( "=" / "*" / "@" ) [channel] :[ "@" / "+" ] [nick] *( " " [ "@" / "+" ] [nick] )
        /// </summary>
        public const string RPL_NAMES = "353";

        /// <summary>
        ///     [mask] [server] :[hopcount] [serverinfo]
        /// </summary>
        public const string RPL_LINKS = "364";

        /// <summary>
        ///     [mask] :End of LINKS list
        /// </summary>
        public const string RPL_ENDOFLINKS = "365";

        /// <summary>
        ///     [channel] :End of NAMES list
        /// </summary>
        public const string RPL_ENDOFNAMES = "366";

        /// <summary>
        ///     [channel] [banmask]
        /// </summary>
        public const string RPL_BANLIST = "367";

        /// <summary>
        ///     [channel] :End of channel ban list
        /// </summary>
        public const string RPLENDOFBANLIST_ = "368";

        /// <summary>
        ///     :[string]
        /// </summary>
        public const string RPL_INFO = "371";

        /// <summary>
        ///     :- [text]
        /// </summary>
        public const string RPL_MOTD = "372";

        /// <summary>
        ///     :End of INFO list
        /// </summary>
        public const string RPL_ENDOFINFO = "374";

        /// <summary>
        ///     :- [server] Args of the day -
        /// </summary>
        public const string RPL_MOTDSTART = "375";

        /// <summary>
        ///     :End of MOTD command
        /// </summary>
        public const string RPL_ENDOFMOTD = "376";

        /// <summary>
        ///     :You are now an IRC operator
        /// </summary>
        public const string RPL_YOUREOPER = "381";

        /// <summary>
        ///     [config file] :Rehashing
        /// </summary>
        public const string RPL_REHASHING = "382";

        /// <summary>
        ///     You are service [servicename]
        /// </summary>
        public const string RPL_YOURESERVICE = "383";

        /// <summary>
        ///     :[username] [ttyline] [hostname]
        /// </summary>
        public const string RPL_USERS = "393";

        /// <summary>
        ///     :End of users
        /// </summary>
        public const string RPL_ENDOFUSERS = "394";

        /// <summary>
        ///     :Nobody logged in
        /// </summary>
        public const string RPL_NOUSERS = "395";


        /* Error responses */

        /// <summary>
        ///     [nickname] :No such nick/channel
        /// </summary>
        public const string ERR_NOSUCHNICK = "401";

        /// <summary>
        ///     [server name] :No such server
        /// </summary>
        public const string ERR_NOSUCHSERVER = "402";

        /// <summary>
        ///     [channel name] :No such channel
        /// </summary>
        public const string ERR_NOSUCHCHANNEL = "403";

        /// <summary>
        ///     [channel name] :Cannot send to channel
        /// </summary>
        public const string ERR_CANNOTSENDTOCHAN = "404";

        /// <summary>
        ///     [channel name] :You have joined too many channels
        /// </summary>
        public const string ERR_TOOMANYCHANNELS = "405";

        /// <summary>
        ///     [nickname] :There was no such nickname
        /// </summary>
        public const string ERR_WASNOSUCHNICK = "406";

        /// <summary>
        ///     [target] :[error code] recipients. [abort message]
        /// </summary>
        public const string ERR_TOOMANYTARGETS = "407";

        /// <summary>
        ///     [service name] :No such service
        /// </summary>
        public const string ERR_NOSUCHSERVICE = "408";

        /// <summary>
        ///     :No origin specified - PING or PONG message missing originator parameter
        /// </summary>
        public const string ERR_NOORIGIN = "409";

        /// <summary>
        ///     :No recipient given ([command])
        /// </summary>
        public const string ERR_NORECIPIENT = "411";

        /// <summary>
        ///     :No text to send
        /// </summary>
        public const string ERR_NOTEXTTOSEND = "412";

        /// <summary>
        ///     [mask] :No toplevel domain specified
        /// </summary>
        public const string ERR_NOTOPLEVEL = "413";

        /// <summary>
        ///     [mask] :Wildcard in toplevel domain
        /// </summary>
        public const string ERR_WILDTOPLEVEL = "414";

        /// <summary>
        ///     [mask] :Bad Server/host mask
        /// </summary>
        public const string ERR_BADMASK = "415";

        /// <summary>
        ///     [command] :Unknown command
        /// </summary>
        public const string ERR_UNKNOWNCOMMAND = "421";

        /// <summary>
        ///     :MOTD File is missing
        /// </summary>
        public const string ERR_NOMOTD = "422";

        /// <summary>
        ///     [server] :No administrative info available
        /// </summary>
        public const string ERR_NOADMININFO = "423";

        /// <summary>
        ///     :File error doing [file op] on [file]
        /// </summary>
        public const string ERR_FILEERROR = "424";

        /// <summary>
        ///     :No nickname given
        /// </summary>
        public const string ERR_NONICKNAMEGIVEN = "431";

        /// <summary>
        ///     [nick] :Erroneous nickname
        /// </summary>
        public const string ERR_ERRONEOUSNICKNAME = "432";

        /// <summary>
        ///     [nick] :Nickname is already in use
        /// </summary>
        public const string ERR_NICKNAMEINUSE = "433";

        /// <summary>
        ///     [nick] :Nickname collision KILL from [user]@[host]
        /// </summary>
        public const string ERR_NICKCOLLISION = "436";

        /// <summary>
        ///     [nick/channel] :Nick/channel is temporarily unavailable
        /// </summary>
        public const string ERR_UNAVALABLERESOURCE = "437";

        /// <summary>
        ///     [nick] [channel] :They aren't on that channel
        /// </summary>
        public const string ERR_USERNOTINCHANNEL = "441";

        /// <summary>
        ///     [channel] :You're not on that channel
        /// </summary>
        public const string ERR_NOTONCHANNEL = "442";

        /// <summary>
        ///     [user] [channel] :is already on channel
        /// </summary>
        public const string ERR_USERONCHANNEL = "443";

        /// <summary>
        ///     [user] :User not logged in
        /// </summary>
        public const string ERR_NOLOGIN = "444";

        /// <summary>
        ///     :SUMMON has been disabled
        /// </summary>
        public const string ERR_SUMMONSDISABLED = "445";

        /// <summary>
        ///     :USERS has been disabled
        /// </summary>
        public const string ERR_USERSDISABLED = "446";

        /// <summary>
        ///     :You have not registered
        /// </summary>
        public const string ERR_NOTREGISTERED = "451";

        /// <summary>
        ///     [command] :Not enough parameters
        /// </summary>
        public const string ERR_NEEDMOREPARAMS = "461";

        /// <summary>
        ///     :Unauthorized command (already registered)
        /// </summary>
        public const string ERR_ALREADYREGISTERED = "462";

        /// <summary>
        ///     :Your host isn't among the privileged
        /// </summary>
        public const string ERR_NOPERMFORHOST = "463";

        /// <summary>
        ///     :Password incorrect
        /// </summary>
        public const string ERR_PASSWDMISMATCH = "464";

        /// <summary>
        ///     :You are banned from this server
        /// </summary>
        public const string ERR_YOUREBANNEDCREEP = "465";

        /// <summary>
        ///     - Sent by a server to a user to inform that access to the server will soon be denied.
        /// </summary>
        public const string ERR_YOUWILLBEBANNED = "466";

        /// <summary>
        ///     [channel] :Channel key already set
        /// </summary>
        public const string ERR_KEYSET = "467";

        /// <summary>
        ///     [channel] :Cannot join channel (+l)
        /// </summary>
        public const string ERR_CHANNELISFULL = "471";

        /// <summary>
        ///     [char] :is unknown mode char to me for [channel]
        /// </summary>
        public const string ERR_UNKNOWNMODE = "472";

        /// <summary>
        ///     [channel] :Cannot join channel (+i)
        /// </summary>
        public const string ERR_INVITEONLYCHAN = "473";

        /// <summary>
        ///     [channel] :Cannot join channel (+b)
        /// </summary>
        public const string ERR_BANNEDFROMCHAN = "474";

        /// <summary>
        ///     [channel] :Cannot join channel (+k)
        /// </summary>
        public const string ERR_BADCHANNELKEY = "475";

        /// <summary>
        ///     [channel] :Bad Channel Mask
        /// </summary>
        public const string ERR_BADCHANMASK = "476";

        /// <summary>
        ///     [channel] :Channel doesn't support modes
        /// </summary>
        public const string ERR_NOCHANMODES = "477";

        /// <summary>
        ///     [channel] [char] :Channel list is full
        /// </summary>
        public const string ERR_BANLISTFULL = "478";

        /// <summary>
        ///     :Permission Denied- You're not an IRC operator
        /// </summary>
        public const string ERR_NOPRIVILEGES = "481";

        /// <summary>
        ///     [channel] :You're not channel operator
        /// </summary>
        public const string ERR_CHANOPPRIVSNEEDED = "482";

        /// <summary>
        ///     :You can't kill a server
        /// </summary>
        public const string ERR_CANTKILLSERVER = "483";

        /// <summary>
        ///     :Your connection is restricted
        /// </summary>
        public const string ERR_RESTRICTED = "484";

        /// <summary>
        ///     :You're not the original channel operator
        /// </summary>
        public const string ERR_UNIQOPPRIVSNEEDED = "485";

        /// <summary>
        ///     :No O-lines for your host
        /// </summary>
        public const string ERR_NOOPERHOST = "491";

        /// <summary>
        ///     :Unknown MODE flag
        /// </summary>
        public const string ERR_UMODEUNKNOWNFLAG = "501";

        /// <summary>
        ///     :Cannot change mode for other user
        /// </summary>
        public const string ERR_USERSDONTMATCH = "502";

        #endregion

        #region METHODS

        public static List<string> CommandsList { get; } =
            new List<string>(typeof(Commands).GetFields().Select(field => field.Name));

        #endregion
    }
}
