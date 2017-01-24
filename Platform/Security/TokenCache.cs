using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading;
using Tuhu.Component.Framework;

namespace Platform.Security
{
    public static class TokenCache
    {
        #region Private Fields

        private static readonly Dictionary<CacheKey, SecurityToken> dicTokens = new Dictionary<CacheKey, SecurityToken>();

        private static readonly ReaderWriterLock tokenLock = new ReaderWriterLock();

        private const int LockInterval = 1000;

        #endregion

        public static SecurityToken GetToken(CacheKey cacheKey)
        {
            SecurityToken token = null;

            tokenLock.AcquireReaderLock(LockInterval);

            try
            {
                if (dicTokens.ContainsKey(cacheKey))
                {
                    dicTokens.TryGetValue(cacheKey, out token);
                }

                return token;
            }
            finally
            {
                tokenLock.ReleaseReaderLock();
            }
        }

        public static void AddToken(CacheKey cacheKey, SecurityToken token)
        {
            tokenLock.AcquireWriterLock(LockInterval);

            try
            {
                dicTokens[cacheKey] = token;
            }
            finally
            {
                tokenLock.ReleaseWriterLock();
            }
        }
    }

    public class CacheKey : IEquatable<CacheKey>, IEqualityComparer<CacheKey>
    {
        public CacheKey(string userName, Uri issuerUri)
        {
            this.UserName = userName;
            this.IssuerUri = issuerUri;
        }

        public string UserName { get; private set; }

        public Uri IssuerUri { get; private set; }

        public bool Equals(CacheKey other)
        {
            return other != null &&
                   string.Equals(this.UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase) &&
                   this.IssuerUri == other.IssuerUri;
        }

        public bool Equals(CacheKey x, CacheKey y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return StringUtils.IngoreCaseCompare(x.UserName, y.UserName) &&
                   x.IssuerUri == y.IssuerUri;
        }

        public int GetHashCode(CacheKey obj)
        {
            if (obj == null)
            {
                return -1;
            }

            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return this.UserName.GetHashCode() & this.IssuerUri.GetHashCode();
        }
    }
}
