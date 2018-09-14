namespace Microsoft.Hpc
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public class ErrorBehaviorAttribute : Attribute, IServiceBehavior
    {
        /// <summary>
        /// Type of component to which this error handled should be bound
        /// </summary>
        private readonly Type errorHandlerType;

        /// <summary>
        /// Initializes a new instance of the ErrorBehaviorAttribute class.
        /// </summary>
        /// <param name="errorHandlerType">Type of component to which this error handled should be bound</param>
        public ErrorBehaviorAttribute(Type errorHandlerType)
        {
            this.errorHandlerType = errorHandlerType;
        }

        /// <summary>
        /// Type of component to which this error handled should be bound
        /// </summary>
        public Type ErrorHandlerType => errorHandlerType;

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="description">
        /// <para>Type: <see cref="System.ServiceModel.Description.ServiceDescription"/></para>
        /// <para>The service description.</para>
        /// </param>
        /// <param name="serviceHostBase">
        /// <para>Type: <see cref="System.ServiceModel.ServiceHostBase"/></para>
        /// <para>The host that is currently being built.</para>
        /// </param>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            IErrorHandler errorHandler;
            try
            {
                errorHandler = (IErrorHandler)Activator.CreateInstance(errorHandlerType);
            }
            catch (MissingMethodException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must have a public empty constructor.", e);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must implement System.ServiceModel.Dispatcher.IErrorHandler.", e);
            }

            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    channelDispatcher.ErrorHandlers.Add(errorHandler);
                }
            }
        }
    }
}
