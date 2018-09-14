using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{    
    [Serializable]
    public class CallResult
    {
        int _code = ErrorCode.Success;
        string _params = string.Empty;

        public CallResult(int code)
            : this(code, string.Empty)
        {
        }

        public CallResult(int code, object param)
            : this(code, param.ToString ())
        {
        }

        public CallResult(int code, string message)
        {
            _code = code;
            _params = message;
        }
    
        public CallResult(Exception e)
        {
            _InitFromException(e);
        }

        public void Throw()
        {
            throw new SchedulerException(_code, _params);            
        }

        static public void LocalThrow(int code)
        {
            throw new SchedulerException(code, string.Empty);
        }

        public int Code 
        {
            get { return _code; }
        }
        
        public string Params
        {
            get { return _params; }
        }
        
        static CallResult _succeeded = new CallResult(ErrorCode.Success);
        
        public static CallResult Succeeded
        {
            get { return _succeeded; }
        }

        void _InitFromException(Exception e)
        {
            if (e is SqlException)
            {
                _code = ErrorCode.Operation_DatabaseException;
                _params = e.Message;
            }
            else if (e is ArgumentOutOfRangeException)
            {
                _code = ErrorCode.Operation_ArgumentOutOfRange;
                _params = e.Message;
            }
            else if (e is System.Security.Principal.IdentityNotMappedException)
            {
                _code = ErrorCode.Operation_AuthenticationFailure;
                _params = SR.InvalidUserName;
            }
            else if (e is System.Security.Authentication.AuthenticationException)
            {
                _code = ErrorCode.Operation_AuthenticationFailure;
                _params = e.Message;
            }
            else if (e is System.Security.Cryptography.CryptographicException)
            {
                _code = ErrorCode.Operation_CryptographyError;
                _params = e.Message;
            }
            else if (e is InvalidOperationException)
            {
                _code = ErrorCode.Operation_InvalidOperation;
                _params = e.Message;
            }
            else if (e is SchedulerException)
            {
                SchedulerException se = e as SchedulerException;

                _code = se.Code;
                _params = se.Params;
            }
            else
            {
                _code = ErrorCode.Operation_UnexpectedException;
                _params = (e.Message == null ? "No exception message" : e.Message);
            }
        }
    }
}
