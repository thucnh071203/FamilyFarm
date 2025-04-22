using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;

namespace FamilyFarm.BusinessLogic.Config
{
    public abstract class FirebaseConnection
    {
        protected FirebaseConnection()
        {
            EnsureFirebaseInitialized();
        }

        private static bool _isInitialized = false;
        private static readonly object _lock = new();

        private void EnsureFirebaseInitialized()
        {
            if (!_isInitialized)
            {
                lock (_lock)
                {
                    if (!_isInitialized)
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "prn221-69738-firebase-adminsdk-syn4i-4dee075804.json");

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
