using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

using Tuhu.Component.Framework.Identity;

namespace Platform.MessageInspector
{
    public class GetThreadIdentityMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var userId = string.Empty;
            if (request.Headers.FindHeader(IdentifierConstant.UserId, IdentifierConstant.Namespace) > 0)
            {
                userId = request.Headers.GetHeader<string>(IdentifierConstant.UserId, IdentifierConstant.Namespace);
            }

            var deviceId = string.Empty;
            if (request.Headers.FindHeader(IdentifierConstant.DeviceId, IdentifierConstant.Namespace) > 0)
            {
                deviceId = request.Headers.GetHeader<string>(IdentifierConstant.DeviceId, IdentifierConstant.Namespace);
            }

            var ipAddress = string.Empty;
            if (request.Headers.FindHeader(IdentifierConstant.IPAddress, IdentifierConstant.Namespace) > 0)
            {
                ipAddress = request.Headers.GetHeader<string>(IdentifierConstant.IPAddress, IdentifierConstant.Namespace);
            }

            var deviceType = string.Empty;
            if (request.Headers.FindHeader(IdentifierConstant.DeviceType, IdentifierConstant.Namespace) > 0)
            {
                deviceType = request.Headers.GetHeader<string>(IdentifierConstant.DeviceType, IdentifierConstant.Namespace);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                ThreadIdentity.Identifier = new UserIdentifier(userId, ipAddress, deviceId, deviceType);
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            // Nothing to do.
        }
    }
}
