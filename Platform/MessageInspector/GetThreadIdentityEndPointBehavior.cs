using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Platform.MessageInspector
{
    public class GetThreadIdentityEndPointBehavior : IEndpointBehavior
    {
        public void Validate(ServiceEndpoint endpoint)
        { }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        { }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new GetThreadIdentityMessageInspector());
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        { }
    }

    public class GetThreadIdentityBehaviorExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new GetThreadIdentityEndPointBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(GetThreadIdentityEndPointBehavior);
            }
        }
    }
}
