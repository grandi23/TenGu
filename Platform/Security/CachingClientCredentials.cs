using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;

using Tuhu.Component.Framework.ExceptionHandling;

namespace Platform.Security
{
    public class CachingClientCredentials : ClientCredentials
    {
        public CachingClientCredentials()
        { }

        protected CachingClientCredentials(ClientCredentials clientCredentials)
            : base(clientCredentials)
        { }

        protected override ClientCredentials CloneCore()
        {
            return new CachingClientCredentials(this);
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new CachingClientCredentialsSecurityTokenManager(this);
        }

        private class CachingClientCredentialsSecurityTokenManager : ClientCredentialsSecurityTokenManager
        {
            public CachingClientCredentialsSecurityTokenManager(CachingClientCredentials clientCredentials)
                : base(clientCredentials)
            { }

            #region SecurityTokenManager

            public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
            {
                var tokenProvider = base.CreateSecurityTokenProvider(tokenRequirement);
                if (this.IsIssuedSecurityTokenRequirement(tokenRequirement))
                {
                    var userName = this.ClientCredentials.UserName.UserName ?? "SYSTEM";
                    tokenProvider = new CachingIssuedSecurityTokenProvider(tokenProvider as IssuedSecurityTokenProvider, userName);
                }

                return tokenProvider;
            }

            #endregion

            private class CachingIssuedSecurityTokenProvider : IssuedSecurityTokenProvider
            {
                private readonly IssuedSecurityTokenProvider issuedSecurityTokenProvider;
                private readonly string userName;

                public CachingIssuedSecurityTokenProvider(IssuedSecurityTokenProvider issuedSecurityTokenProvider, string userName)
                {
                    this.issuedSecurityTokenProvider = issuedSecurityTokenProvider;
                    this.userName = userName;

                    this.CacheIssuedTokens = issuedSecurityTokenProvider.CacheIssuedTokens;
                    this.IdentityVerifier = issuedSecurityTokenProvider.IdentityVerifier;
                    this.IssuedTokenRenewalThresholdPercentage = issuedSecurityTokenProvider.IssuedTokenRenewalThresholdPercentage;
                    this.IssuerAddress = issuedSecurityTokenProvider.IssuerAddress;
                    this.IssuerBinding = issuedSecurityTokenProvider.IssuerBinding;
                    foreach (var channelBehavior in issuedSecurityTokenProvider.IssuerChannelBehaviors)
                    {
                        this.IssuerChannelBehaviors.Add(channelBehavior);
                    }
                    this.KeyEntropyMode = issuedSecurityTokenProvider.KeyEntropyMode;
                    this.MaxIssuedTokenCachingTime = issuedSecurityTokenProvider.MaxIssuedTokenCachingTime;
                    this.MessageSecurityVersion = issuedSecurityTokenProvider.MessageSecurityVersion;
                    this.SecurityAlgorithmSuite = issuedSecurityTokenProvider.SecurityAlgorithmSuite;
                    this.SecurityTokenSerializer = issuedSecurityTokenProvider.SecurityTokenSerializer;
                    this.TargetAddress = issuedSecurityTokenProvider.TargetAddress;
                    foreach (var parameter in issuedSecurityTokenProvider.TokenRequestParameters)
                    {
                        this.TokenRequestParameters.Add(parameter);
                    }

                    this.issuedSecurityTokenProvider.Open();
                }

                ~CachingIssuedSecurityTokenProvider()
                {
                    this.issuedSecurityTokenProvider.Close();
                }

                protected override SecurityToken GetTokenCore(TimeSpan timeout)
                {
                    SecurityToken token;
                    if (this.CacheIssuedTokens)
                    {
                        var cacheKey = new CacheKey(userName, this.issuedSecurityTokenProvider.IssuerAddress.Uri);
                        token = TokenCache.GetToken(cacheKey);
                        if (token == null || token.ValidTo.ToUniversalTime() < DateTime.UtcNow)
                        {
                            try
                            {
                                token = this.issuedSecurityTokenProvider.GetToken(timeout);
                            }
                            catch (FaultException<FaultData>)
                            {
                                throw;
                            }
                            catch (FaultException faultException)
                            {
                                var faultMessage = faultException.CreateMessageFault();
                                if (faultMessage.HasDetail)
                                {
                                    var faultData = new DataContractSerializer(typeof(FaultData)).ReadObject(faultMessage.GetReaderAtDetailContents()) as FaultData;
                                    throw new FaultException<FaultData>(faultData, faultMessage.Reason, faultMessage.Code);
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            TokenCache.AddToken(cacheKey, token);
                        }
                    }
                    else
                    {
                        token = this.issuedSecurityTokenProvider.GetToken(timeout);
                    }

                    return token;
                }
            }
        }
    }
}
