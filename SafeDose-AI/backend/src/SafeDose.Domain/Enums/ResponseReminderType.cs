using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Domain.Enums
{
    public enum ResponseReminderType : byte
    {
        Taken = 1,      // تم أخذ الجرعة
        Rejected = 2,   // رفض أخذ الجرعة
        Ignored = 3     // تجاهل الإشعار
    }

}
