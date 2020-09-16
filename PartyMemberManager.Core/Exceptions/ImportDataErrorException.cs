using System;
using System.Collections.Generic;
using System.Text;

namespace PartyMemberManager.Core.Exceptions
{
    /// <summary>
    /// 导入数据错误异常
    /// </summary>
    public class ImportDataErrorException:PartyMemberException
    {
        public ImportDataErrorException(string message) : base(message)
        {
        }

        public ImportDataErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public ImportDataErrorException(string key, string message) : base(message)
        {
        }

        public ImportDataErrorException(string key, string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
