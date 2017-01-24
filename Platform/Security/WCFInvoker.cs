using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Threading.Tasks;
using System.Xml;

using Tuhu.Component.Framework;
using Tuhu.Component.Framework.ExceptionHandling;
using Tuhu.Component.Framework.Extension;
using Tuhu.Component.Framework.Identity;

namespace Platform.Security
{
    /// <summary>
    /// Helper class for WCF.
    /// </summary>
    public static class WCFInvoker
    {
        public static void Invoke<TChannel, TException>(string endpointName, Action<TChannel> action, ILog logger)
            where TChannel : class
            where TException : TuhuBizException
        {
            Invoke<TChannel, TException>(string.Empty, endpointName, action, logger);
        }

        public static TResult Invoke<TChannel, TResult, TException>(string endpointName, Func<TChannel, TResult> func, ILog logger)
            where TChannel : class
            where TResult : class
            where TException : TuhuBizException
        {
            return Invoke<TChannel, TResult, TException>(string.Empty, endpointName, func, logger);
        }

        public static void Invoke<TChannel, TException>(string applicationName, string endpointName, Action<TChannel> action, ILog logger)
            where TChannel : class
            where TException : TuhuBizException
        {
            ParameterChecker.CheckNullOrEmpty(endpointName, "endpointName");
            ParameterChecker.CheckNull(action, "action");

            using (var channelFactory = new ChannelFactory<TChannel>(endpointName))
            {
                SetChannelCredential(channelFactory, applicationName);
                var channel = channelFactory.CreateChannel();

                try
                {
                    action(channel);
                }
                catch (FaultException<FaultData> innerEx)
                {
                    var exception = (TException)Activator.CreateInstance(typeof(TException), innerEx.Detail.Id, innerEx.Detail.Message);

                    throw exception;
                }
                catch (FaultException innerEx)
                {
                    var faultMessage = innerEx.CreateMessageFault();
                    if (faultMessage.HasDetail)
                    {
                        var faultData = faultMessage.GetDetail<FaultData>();
                        if (faultData != null)
                        {
                            throw (TException)Activator.CreateInstance(typeof(TException), faultData.Id, faultData.Message);
                        }
                    }

                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "系统出错,联系IT", innerEx);
                }
                catch (MessageSecurityException innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "安全验证出错或客户端配置出错", innerEx);
                }
                catch (CommunicationException innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx, "CommunicationException");
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "网络出错,请重试", innerEx);
                }
                catch (TimeoutException innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx, "TimeoutException");
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "网络出错,请重试", innerEx);
                }
                catch (Exception innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "系统出错,联系IT", innerEx);
                }
                finally
                {
                    CloseChannel(channel as IChannel);
                }
            }
        }

        public static TResult Invoke<TChannel, TResult, TException>(string applicationName, string endpointName, Func<TChannel, TResult> func, ILog logger)
            where TChannel : class
            where TResult : class
            where TException : TuhuBizException
        {
            ParameterChecker.CheckNullOrEmpty(endpointName, "endpointName");
            ParameterChecker.CheckNull(func, "func");

            using (var channelFactory = new ChannelFactory<TChannel>(endpointName))
            {
                SetChannelCredential(channelFactory, applicationName);
                var channel = channelFactory.CreateChannel();

                try
                {
                    return func(channel);
                }
                catch (FaultException<FaultData> innerEx)
                {
                    var exception = (TException)Activator.CreateInstance(typeof(TException), innerEx.Detail.Id, innerEx.Detail.Message);

                    throw exception;
                }
                catch (FaultException innerEx)
                {
                    var faultMessage = innerEx.CreateMessageFault();
                    if (faultMessage.HasDetail)
                    {
                        var faultData = faultMessage.GetDetail<FaultData>();
                        if (faultData != null)
                        {
                            throw (TException)Activator.CreateInstance(typeof(TException), faultData.Id, faultData.Message);
                        }
                    }

                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "系统出错,联系IT", innerEx);
                }
                catch (MessageSecurityException innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "安全验证出错或客户端配置出错", innerEx);
                }
                catch (CommunicationException innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "网络出错,请重试", innerEx);
                }
                catch (TimeoutException innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "网络出错,请重试", innerEx);
                }
                catch (Exception innerEx)
                {
                    if (logger != null)
                    {
                        logger.Log(Level.Error, innerEx, innerEx.Message);
                    }

                    throw (TException)Activator.CreateInstance(typeof(TException), 0, "系统出错,联系IT", innerEx);
                }
                finally
                {
                    CloseChannel(channel as IChannel);
                }
            }
        }

        public static async Task InvokeAsync<TChannel, TException>(string endpointName, Action<TChannel> action, ILog logger)
            where TChannel : class
            where TException : TuhuBizException
        {
            await Task.Factory.StartNew(() =>
            {
                Invoke<TChannel, TException>(endpointName, action, logger);
            });
        }

        public static async Task<TResult> InvokeAsync<TChannel, TResult, TException>(string endpointName, Func<TChannel, TResult> func, ILog logger)
            where TChannel : class
            where TResult : class
            where TException : TuhuBizException
        {
            return await Task<TResult>.Factory.StartNew(() =>
            {
                return Invoke<TChannel, TResult, TException>(endpointName, func, logger);
            });
        }

        private static void SetChannelCredential(ChannelFactory channelFactory, string applicationName)
        {
            ParameterChecker.CheckNull(channelFactory, "channelFactory");
            channelFactory.Credentials.UserName.UserName = ThreadIdentity.Operator.Name;

            var customBinding = channelFactory.Endpoint.Binding as CustomBinding;
            if (customBinding == null)
            {
                return;
            }

            var securityBindingElement = customBinding.Elements.Find<SecurityBindingElement>();
            if (securityBindingElement == null)
            {
                return;
            }

            IssuedSecurityTokenParameters issuedTokenParams;
            var transportSecurityBindingElement = securityBindingElement as TransportSecurityBindingElement;
            if (transportSecurityBindingElement != null)
            {
                issuedTokenParams = transportSecurityBindingElement.EndpointSupportingTokenParameters.Endorsing.FirstOrDefault()
                    as IssuedSecurityTokenParameters;
            }
            else
            {
                var symmetricSecurityBindingElement = securityBindingElement as SymmetricSecurityBindingElement;
                var protectionTokenParameter = symmetricSecurityBindingElement.ProtectionTokenParameters
                    as SecureConversationSecurityTokenParameters;
                if (protectionTokenParameter != null)
                {
                    issuedTokenParams = protectionTokenParameter.BootstrapSecurityBindingElement.EndpointSupportingTokenParameters.Endorsing.FirstOrDefault()
                        as IssuedSecurityTokenParameters;
                }
                else
                {
                    issuedTokenParams = symmetricSecurityBindingElement.EndpointSupportingTokenParameters.Endorsing.FirstOrDefault() as IssuedSecurityTokenParameters;
                }
            }

            if (issuedTokenParams != null)
            {
                var credentialElement = new XmlDocument().CreateElement(SecurityCredentials.CredentialsRoot);
                credentialElement.AppendChild(SecurityCredentials.UserId, ThreadIdentity.Operator.Name);
                credentialElement.AppendChild(SecurityCredentials.Password, ThreadIdentity.Operator.Password);
                credentialElement.AppendChild(SecurityCredentials.DeviceId, ThreadIdentity.Operator.DeviceId);
                credentialElement.AppendChild(SecurityCredentials.DeviceType, ThreadIdentity.Operator.DeviceType);
                credentialElement.AppendChild(SecurityCredentials.IPAddress, ThreadIdentity.Operator.IPAddress);
                if (!string.IsNullOrEmpty(applicationName))
                {
                    credentialElement.AppendChild(SecurityCredentials.Application, applicationName);
                }

                issuedTokenParams.AdditionalRequestParameters.Add(credentialElement);
            }
        }

        private static void CloseChannel(IChannel channel)
        {
            ParameterChecker.CheckNull(channel, "channel");

            try
            {
                channel.Close();
            }
            catch
            {
                channel.Abort();
            }
        }
    }
}
