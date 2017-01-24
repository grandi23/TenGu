using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

using Tuhu.Component.Framework.Identity;

namespace Platform.MessageInspector
{
    public class AttachThreadIdentityMessageInspector : IClientMessageInspector
    {
        private const string LoginAction = "biz.tuhu.cn/ILogin/Login";

        #region IClientMessageInspector

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (!string.Equals(LoginAction, request.Headers.Action, StringComparison.CurrentCultureIgnoreCase))
            {
                request.Headers.Add(MessageHeader.CreateHeader(IdentifierConstant.UserId, IdentifierConstant.Namespace, ThreadIdentity.Operator.Name));
                request.Headers.Add(MessageHeader.CreateHeader(IdentifierConstant.DeviceId, IdentifierConstant.Namespace, ThreadIdentity.Operator.DeviceId));
                request.Headers.Add(MessageHeader.CreateHeader(IdentifierConstant.DeviceType, IdentifierConstant.Namespace, ThreadIdentity.Operator.DeviceType));
                request.Headers.Add(MessageHeader.CreateHeader(IdentifierConstant.IPAddress, IdentifierConstant.Namespace, ThreadIdentity.Operator.IPAddress));
                request.Headers.Add(MessageHeader.CreateHeader(IdentifierConstant.Token, IdentifierConstant.Namespace, DateTime.Now));
            }

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // Nothing to do.
        }

        #endregion
    }
}
