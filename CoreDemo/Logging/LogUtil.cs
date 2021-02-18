using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreDemo.Logging
{
    /// <summary>
    /// Logger event codes
    /// </summary>
public static class EvtCodes
    {
        public const int evtLogInOk = 1000;
        public const int evtLogInFail = 1001;
        public const int evtLogInLocked = 1002;
        public const int evtLogOut = 1003;
        public const int evtPwdChangeOK = 1004;
        public const int evtPwdChangeFail = 1005;
        public const int evtPwdChangeDemand = 1006;

        public const int evtForbidden = 1100;

        public const int evtUserAddOk = 1200;
        public const int evtUserAddFail = 1201;
        public const int evtUserDeleteOk = 1202;
        public const int evtUserDeleteFail = 1203;

        public const int evtWorkAddOk = 1300;
        public const int evtWorkAddFail = 1301;
        public const int evtWorkDeleteOk = 1302;
        public const int evtWorkDeleteFail = 1303;
        public const int evtWorkChangeOk = 1304;
        public const int evtWorkChangeFail = 1305;
            
        public const int evtComposerAddOk = 1400;
        public const int evtComposerAddFail = 1401;
        public const int evtComposerDeleteOk = 1402;
        public const int evtComposerDeleteFail = 1403;
        public const int evtComposerChangeOk = 1404;
        public const int evtComposerChangeFail = 1405;
    }
}
