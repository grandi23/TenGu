using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Platform.MessageInspector
{
    public class AttachThreadIdentityEndPointBehavior : IEndpointBehavior
    {
        #region IEndpointBehavior

        public void Validate(ServiceEndpoint endpoint)
        {

        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new AttachThreadIdentityMessageInspector());
        }

        #endregion
    }

    public class AttachThreadIdentityBehaviorExtension : BehaviorExtensionElement
    {
        #region BehaviorExtensionElement

        protected override object CreateBehavior()
        {
            return new AttachThreadIdentityEndPointBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(AttachThreadIdentityEndPointBehavior);
            }
        }

        #endregion
    }
}
