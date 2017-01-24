using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Platform.MessageInspector
{
    public class WatchServerThresholdEndpointBehavior : IEndpointBehavior
    {
        #region Ctor.

        private WatchServerThresholdConfig config;
        public WatchServerThresholdEndpointBehavior(WatchServerThresholdConfig config)
        {
            this.config = config;
        }

        #endregion

        #region IEndpointBehavior

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new WatchServerThresholdMessageInspector(this.config));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new WatchServerThresholdMessageInspector(this.config));
        }

        public void Validate(ServiceEndpoint endpoint)
        { }

        #endregion
    }

    public class WatchServerThresholdBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get
            {
                return typeof(WatchServerThresholdEndpointBehavior);
            }
        }

        protected override object CreateBehavior()
        {
            var config = new WatchServerThresholdConfig(this.Threshold, this.EnableDump, this.DumpCmd, this.DumpInterval);

            return new WatchServerThresholdEndpointBehavior(config);
        }

        [ConfigurationProperty("threshold")]
        public int Threshold
        {
            get
            {
                return (int)this["threshold"];
            }
        }

        [ConfigurationProperty("enableDump")]
        public bool EnableDump
        {
            get
            {
                return (bool)this["enableDump"];
            }
        }

        [ConfigurationProperty("dumpCmd")]
        public string DumpCmd
        {
            get
            {
                return (string)this["dumpCmd"];
            }
        }

        [ConfigurationProperty("dumpInterval")]
        public int DumpInterval
        {
            get
            {
                return (int)this["dumpInterval"];
            }
        }
    }
}
