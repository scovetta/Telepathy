namespace Microsoft.Hpc.Scheduler.Properties
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para>Defines an exception raised by the scheduler.</para>
    /// </summary>
    [Serializable]
    public class SchedulerException : Exception
    {
        private string errorCodeSerializationString = "_errorCode";
        private string errorParamsSerializationString = "_errorParams";

        /// <summary>
        ///   <para>The <see cref="Microsoft.Hpc.Scheduler.Properties.ErrorCode" /> constant that identifies the error and its message string.</para>
        /// </summary>
        /// <value>
        ///   <para>The <see cref="Microsoft.Hpc.Scheduler.Properties.ErrorCode" /> constant that identifies the error.</para>
        /// </value>
        public int Code { get; private set; } = ErrorCode.Success;

        /// <summary>
        ///   <para>Retrieves the insertion strings used for the message.</para>
        /// </summary>
        /// <value>
        ///   <para>A string that contains a list of insert strings. The insert strings are delimited using three vertical bars (string1|||string2).</para>
        /// </value>
        public string Params { get; private set; } = string.Empty;

        /// <summary>
        ///   <para>Initialize a new, empty instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.SchedulerException" /> class.</para>
        /// </summary>
        public SchedulerException()
        {
            this.HResult = ErrorCode.UnknownError;
            this.Code = ErrorCode.UnknownError;
        }

        /// <summary>
        ///   <para>Initialize a new, instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.SchedulerException" /> class using the specified message.</para>
        /// </summary>
        /// <param name="Exception">
        ///   <para>String to use as the <see cref="System.Exception.Message" /> value.</para>
        /// </param>
        public SchedulerException(string message) : base(message)
        {
            this.HResult = ErrorCode.UnknownError;
            this.Code = ErrorCode.UnknownError;
        }

        /// <summary>
        ///   <para>Initialize a new, instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.SchedulerException" /> class using the specified message and inner exception.</para>
        /// </summary>
        /// <param name="Exception">
        ///   <para>String to use as the <see cref="System.Exception.Message" /> value.</para>
        /// </param>
        /// <param name="inner">
        ///   <para>The inner exception.</para>
        /// </param>
        public SchedulerException(string message, Exception inner)
            : base(message, inner)
        {
            this.HResult = ErrorCode.UnknownError;
            this.Code = ErrorCode.UnknownError;
        }

        /// <summary>
        ///   <para>Initialize a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.SchedulerException" /> class using the specified error code and insert arguments.</para>
        /// </summary>
        /// <param name="errorCode">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.ErrorCode" /> constant the identifies the error and its message string. </para>
        /// </param>
        /// <param name="errorParams">
        ///   <para>A string that contains a list of insert strings. The insert strings are delimited using three vertical bars (string1|||string2). </para>
        ///   <para>The insert string correspond directly to the insert sequences defined in the message string. For example, the first insert string 
        /// corresponds to the %1 insert sequence in the message string; the second insert string corresponds to the %2 insert sequence; and so on.</para>
        /// </param>
        public SchedulerException(int errorCode, string errorParams)
            : base(ErrorCode.ToString(errorCode, errorParams))
        {
            this.Code = errorCode;
            this.Params = errorParams;

            this.HResult = errorCode;
        }

        /// <summary>
        ///   <para>Initialize a new, instance of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.SchedulerException" /> class using the specified serialization information and streaming context.</para> 
        /// </summary>
        /// <param name="info">
        ///   <para>A <see cref="System.Runtime.Serialization.SerializationInfo" /> object used to serialize the exception.</para>
        /// </param>
        /// <param name="context">
        ///   <para>A 
        /// 
        /// <see cref="System.Runtime.Serialization.StreamingContext" /> object that describes the source and destination of a given serialized stream, and provides an additional caller-defined context.</para> 
        /// </param>
        protected SchedulerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            try
            {
                this.Code = info.GetInt32(this.errorCodeSerializationString);
            }
            catch (SerializationException)
            {
                // if the exception was serialized by an older server then
                // it may not have the serialized error code
            }

            try
            {
                this.Params = info.GetString(this.errorParamsSerializationString);
            }
            catch (SerializationException)
            {
                // if the exception was serialized by an older server then
                // it may not have the serialized error param
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="info">
        ///   <para />
        /// </param>
        /// <param name="context">
        ///   <para />
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(this.errorCodeSerializationString, this.Code);
            info.AddValue(this.errorParamsSerializationString, this.Params);
        }
    }
}
