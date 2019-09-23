// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;

    /// <summary>
    /// The CSharp Usage Utlity Functions
    /// </summary>
    public static class CSharpUsageUtility
    {
        /// <summary>
        /// This function is used to create a disposable object safely, if any exception occurs in [initialize], 
        /// the object will be disposed
        /// Fix CA2000 : http://msdn.microsoft.com/en-us/library/ms182289.aspx
        /// </summary>
        /// <typeparam name="T">The type of the object to be created</typeparam>
        /// <param name="newT">The function of create T</param>
        /// <param name="initialize">The actions to do after creation</param>
        /// <param name="trace">The actions(offen as trace) to do when exception occurs</param>
        /// <returns>The safe created object</returns>
        /// <remarks>
        /// Sample Usage : 
        ///     SafeCreateDisposableObejct<Message>(
        ///     () => Message.Create(id, content),
        ///     (message) => 
        ///         {
        ///             DoActions(message);
        ///         },
        ///     (exception) => Trace.WriteLine(exception)
        ///     );
        /// </remarks>
        public static T SafeCreateDisposableObject<T>(Func<T> newT, Action<T> initialize, Action<Exception> trace = null) where T : class,IDisposable
        {
            T temp = null;
            T obj = null;
            try
            {
                temp = newT();
                initialize(temp);
                obj = temp;
                temp = null;
                return obj;
            }
            catch (Exception e)
            {
                if (trace != null)
                {
                    trace(e);
                }

                throw;
            }
            finally
            {
                if (temp != null)
                {
                    temp.Dispose();
                }
            }
        }

        /// <summary>
        /// This function is used to create a disposed object which is taken a disposable object in its Constructor,
        /// if any exception happens, object would be disposed safely
        /// if we use with 2 using like : 
        ///         using(MemoryStream ms = new MemoryStream())
        ///         using(StreamReader reader = new StreamReader(ms))
        /// will cause CA2202
        ///
        /// Fix CA2000 : http://msdn.microsoft.com/en-us/library/ms182289.aspx
        /// Fix CA2202 : http://msdn.microsoft.com/en-us/library/ms182334.aspx
        /// </summary>
        /// <typeparam name="T">The type of the object using in constructor</typeparam>
        /// <typeparam name="U">The type of the object to be created</typeparam>
        /// <param name="newT">The function of create T</param>
        /// <param name="newU">The function of create U</param>
        /// <param name="tObject">The object instance of T to be filled</param>
        /// <param name="trace">The actions(offen as trace) to do when exception occurs</param>
        /// <returns>The safe created object</returns>
        /// <remarks>
        /// Sample Usage :
        ///     MemoryStream ms = null;
        ///     using(StreamReader reader = SafeCreateWrappedDisposableObject<StreamReader,MemoryStream>(
        ///         () => new MemoryStream(),   // if have some actions to deal with this Object, please use SafeCreateDisposableObejct to create it
        ///         (ms) => new StreamReader(ms),
        ///         out ms,
        ///         (exception) => Trace.WriteLine(exception)
        ///     ))
        ///     {
        ///         reader.Run();
        ///     }
        /// 
        /// </remarks>
        public static U SafeCreateWrappedDisposableObject<T, U>(Func<T> newT, Func<T, U> newU, out T tObject, Action<Exception> trace = null)
            where T : class , IDisposable
            where U : class, IDisposable
        {
            T t = null;
            try
            {
                t = newT();
                U u = newU(t);
                tObject = t;
                t = null;
                return u;
            }
            catch (Exception e)
            {
                if (trace != null)
                {
                    trace(e);
                }

                throw;
            }
            finally
            {
                if (t != null)
                {
                    t.Dispose();
                }
            }
        }

        /// <summary>
        /// A overload function which OUT parameter is not being taken
        /// </summary>
        public static U SafeCreateWrappedDisposableObject<T, U>(Func<T> newT, Func<T, U> newU, Action<Exception> trace = null)
            where T : class , IDisposable
            where U : class, IDisposable
        {
            T t;
            return CSharpUsageUtility.SafeCreateWrappedDisposableObject<T, U>(newT, newU, out t, trace);
        }

        /// <summary>
        /// Safely dispose an instance of type T, will set the reference to null
        /// regardless dispose succeeded or not.
        /// </summary>
        /// <typeparam name="T">
        /// the type of instance to be disposed, must be an IDisposable
        /// </typeparam>
        /// <param name="instance">
        /// indicating the reference of the instance to be disposed
        /// </param>
        public static void SafeDisposeObject<T>(ref T instance) where T : class,IDisposable
        {
            if (instance != null)
            {
                try
                {
                    instance.Dispose();
                }
                catch
                {
                }

                instance = null;
            }
        }
    }

}