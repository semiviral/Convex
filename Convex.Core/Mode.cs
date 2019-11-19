// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

namespace Convex.Core
{
    public class Mode
    {
        public Mode(char identifier, string syntax = "")
        {
            Identifier = identifier;
            Syntax = syntax;
        }

        #region MEMBERS

        // Several modes mean different things on
        // separate servers, so this may require some
        // tinkering when connecting to a different server.
        //
        // You can probably find your server's MODE specs by
        // Googling them or asking the admin for documentation.
        // 
        // todo: add mode modularity
        //
        // Notes on modes
        // =============
        //
        // A lot of general and important modes are missing.
        // I'll add them... eventually.
        //
        // todo add common missing modes
        //
        // Mode functionality beyond per-basis user recognition 
        // isn't very important.

        public static Mode Ban = new Mode('b', "MODE %c +b %n!%i@%h");

        public static Mode BanException = new Mode('e', "MODE %c +e %n!%i@%h");

        public static Mode NoColor = new Mode('c', "MODE %c +e");

        public static Mode FloodLimit = new Mode('f', "MODE %c +e %p");

        public static Mode JoinThrottle = new Mode('j', "MODE %c +e %p:%p2");

        // Sets limit to number of users in a channel
        public static Mode Limit = new Mode('l', "MODE %c +e %p");

        public static Mode ModeratedChannel = new Mode('m', "MODE %c +e");

        // Considered obsolete, alt: SECRET
        public static Mode Private = new Mode('p', "MODE %c +p");

        public static Mode Secret = new Mode('s', "MODE %c +s");

        public static Mode SecuredOnly = new Mode('z', "MODE %channel +z");

        public static Mode NoCtcp = new Mode('c', "MODE %channel +C");

        public static Mode StripBadWords = new Mode('G', "MODE %c +G");

        // Allows user to talk in +M channels
        public static Mode Voice = new Mode('v', "MODE %c +v %n");

        // Blocks users from using /KNOCK to to try and acces a keyword locked channel
        public static Mode NoKnock = new Mode('K', "MODE %c +K");

        public static Mode NoNickChange = new Mode('N', "MODE %c +N");

        public static Mode NoKicks = new Mode('Q', "MODE %c +Q");

        // Only registered users may join channel
        public static Mode RegOnly = new Mode('R', "MODE %c +R");

        // This channel mode will remove all client color codes from messages in your channel.
        public static Mode Strip = new Mode('S', "MODE %c +S");

        // This blocks users from sending NOTICE's to the channel
        public static Mode NoNotice = new Mode('T', "MODE %c +T");

        // This mode prevents users from sending channel invites to users outside the channel.
        public static Mode NoInvites = new Mode('V', "MODE %c +VarManagement");

        #endregion

        #region MEMBERS

        public char Identifier { get; }

        // public string Translation { get; }
        public string Syntax { get; }

        #endregion
    }
}