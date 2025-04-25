using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Configuration;

namespace FamilyFarm.BusinessLogic.Config
{
    public abstract class FirebaseConnection
    {
        public FirebaseConnection(IConfiguration configuration)
        {
            EnsureFirebaseInitialized(configuration);
        }

        private static bool _isInitialized = false;
        private static readonly object _lock = new();

        protected static void EnsureFirebaseInitialized(IConfiguration configuration)
        {
            if (!_isInitialized)
            {
                lock (_lock)
                {
                    if (!_isInitialized)
                    {
                        var path = configuration["Firebase:CredentialsPath"];

                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(path)
                        });

                        _isInitialized = true;
                    }
                }
            }
        }
    }
}
