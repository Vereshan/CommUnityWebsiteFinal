using CommUnityWeb.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;

public class Firebase
{
    private readonly FirestoreDb _firestoreDb;

    public Firebase()
    {
        string pathToJson = Environment.GetEnvironmentVariable("FIRESTORE_COMMUNITY_CREDENTIALS");

        if (string.IsNullOrEmpty(pathToJson))
        {
            throw new InvalidOperationException("The FIRESTORE_COMMUNITY_CREDENTIALS environment variable is not set.");
        }

        if (!System.IO.File.Exists(pathToJson))
        {
            throw new InvalidOperationException($"The file at {pathToJson} does not exist.");
        }

        try
        {
            GoogleCredential credential = GoogleCredential.FromFile(pathToJson);
            credential = credential.CreateScoped(new[] { "https://www.googleapis.com/auth/cloud-platform" });

            ChannelCredentials channelCredentials = credential.ToChannelCredentials();
            FirestoreClientBuilder clientBuilder = new FirestoreClientBuilder
            {
                ChannelCredentials = channelCredentials
            };
            FirestoreClient client = clientBuilder.Build();
            _firestoreDb = FirestoreDb.Create("community-8aa64", client);

            // Initialize FirebaseApp once in the app's lifecycle
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(pathToJson)
                });
            }

            Console.WriteLine("FirestoreDb and FirebaseApp successfully created.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during Firestore or Firebase initialization: {ex.Message}");
            throw;
        }
    }


    // Get pending Service Providers from Firestore
    public async Task<IEnumerable<UserRequest>> GetPendingServiceProvidersAsync()
    {
        var users = new List<UserRequest>();
        CollectionReference usersRef = _firestoreDb.Collection("ServiceProviders");
        QuerySnapshot snapshot = await usersRef.WhereEqualTo("status", "PENDING").GetSnapshotAsync();

        Console.WriteLine($"Documents fetched: {snapshot.Documents.Count}");

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                var user = document.ConvertTo<UserRequest>();
              
                // Add 5 hours to the DateOfBirth
                user.DateOfBirth = user.DateOfBirth.AddHours(5);
                Console.WriteLine($"Fetched User - ID: {user.IdNumber}, Name: {user.Name}, Status: {user.Status}, DateOfBirth: {user.DateOfBirth}");
                users.Add(user);
            }
        }

        Console.WriteLine($"Total fetched pending users: {users.Count}");
        return users;
    }

    // Get pending Consumers from Firestore
    public async Task<IEnumerable<UserRequest>> GetPendingConsumersAsync()
    {
        var users = new List<UserRequest>();
        CollectionReference usersRef = _firestoreDb.Collection("Consumer");
        QuerySnapshot snapshot = await usersRef.WhereEqualTo("status", "PENDING").GetSnapshotAsync();

        Console.WriteLine($"Documents fetched: {snapshot.Documents.Count}");

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                var user = document.ConvertTo<UserRequest>();
              
                // Add 5 hours to the DateOfBirth
                user.DateOfBirth = user.DateOfBirth.AddHours(5);
                Console.WriteLine($"Fetched User - ID: {user.IdNumber}, Name: {user.Name}, Status: {user.Status}, DateOfBirth: {user.DateOfBirth}");
                users.Add(user);
            }
        }

        Console.WriteLine($"Total fetched pending users: {users.Count}");
        return users;
    }

    // New method to get a user by ID number
    public async Task<UserRequest> GetConsumerByIdNumberAsync(string idNumber)
    {
        // Adjust the query to find the user based on the IdNumber field
        CollectionReference usersRef = _firestoreDb.Collection("Consumer");
        QuerySnapshot snapshot = await usersRef.WhereEqualTo("idNumber", idNumber).GetSnapshotAsync();

        if (snapshot.Documents.Count > 0)
        {
            DocumentSnapshot document = snapshot.Documents[0]; // Assuming IdNumber is unique
            var user = document.ConvertTo<UserRequest>();
            user.Name = user.Name?.ToUpper();
            user.Surname = user.Surname?.ToUpper();
            Console.WriteLine($"Fetched User - ID: {user.IdNumber}, Name: {user.Name}, Status: {user.Status}");
            return user;
        }
        else
        {
            Console.WriteLine($"No user found with ID Number: {idNumber}");
            return null; // Or throw an exception if you prefer
        }
    }

    public async Task<UserRequest> GetServiceProviderByIdNumberAsync(string idNumber)
    {
        CollectionReference usersRef = _firestoreDb.Collection("ServiceProviders");
        QuerySnapshot snapshot = await usersRef.WhereEqualTo("idNumber", idNumber).GetSnapshotAsync();

        if (snapshot.Documents.Count > 0)
        {
            DocumentSnapshot document = snapshot.Documents[0]; // Assuming IdNumber is unique
            var user = document.ConvertTo<UserRequest>();
            user.Name = user.Name?.ToUpper();
            user.Surname = user.Surname?.ToUpper();
            Console.WriteLine($"Fetched User - ID: {user.IdNumber}, Name: {user.Name}, Status: {user.Status}");
            return user;
        }
        else
        {
            Console.WriteLine($"No user found with ID Number: {idNumber}");
            return null; // Or throw an exception if you prefer
        }
    }

    public async Task<bool> UpdateUserStatusAsync(string idNumber, string newStatus, string userType)
    {
        try
        {
            // Determine the collection based on user type (ServiceProvider or Consumer)
            string collectionName = userType == "ServiceProvider" ? "ServiceProviders" : "Consumer";
            CollectionReference usersRef = _firestoreDb.Collection(collectionName);

            // Fetch the user document by IdNumber
            QuerySnapshot snapshot = await usersRef.WhereEqualTo("idNumber", idNumber).GetSnapshotAsync();

            if (snapshot.Documents.Count > 0)
            {
                DocumentSnapshot document = snapshot.Documents[0]; // Assuming IdNumber is unique
                DocumentReference userRef = document.Reference;

                // Update the Status field to newStatus
                Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "status", newStatus }
            };

                await userRef.UpdateAsync(updates);
                Console.WriteLine($"User {idNumber} status updated to {newStatus}");
                return true;
            }
            else
            {
                Console.WriteLine($"No user found with ID Number: {idNumber}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user status: {ex.Message}");
            return false;
        }
    }

    public async Task<(UserLoginModel, string)> AuthenticateAdminAsync(string email, string password)
    {
        Console.WriteLine($"Attempting to authenticate email: {email}"); // Log the email being checked

        CollectionReference adminsRef = _firestoreDb.Collection("Admins");
        QuerySnapshot snapshot = await adminsRef.WhereEqualTo("Email", email).GetSnapshotAsync();

        if (snapshot.Documents.Count > 0)
        {
            DocumentSnapshot document = snapshot.Documents[0];
            var user = document.ConvertTo<UserLoginModel>();

            Console.WriteLine("Admin found. Checking password...");

            // Check if the password matches
            if (user.Password == password) // Ensure that in production, passwords should be hashed
            {
                Console.WriteLine("Authentication successful.");
                return (user, null); // Authentication successful, no error message
            }
            else
            {
                Console.WriteLine("Invalid password.");
                return (null, "Invalid password."); // Invalid password
            }
        }
        else
        {
            Console.WriteLine($"Admin not found with the provided email: {email}.");
            return (null, "Admin not found with the provided email."); // No user found
        }
    }

    public async Task SendNotificationAsync(string token, string title, string body)
    {
        try
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            Console.WriteLine($"Successfully sent message: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending FCM notification: {ex.Message}");
        }
    }


    public async Task<IEnumerable<UserRequest>> GetUsersByCollectionAsync(string collectionName)
    {
        var users = new List<UserRequest>();
        CollectionReference usersRef = _firestoreDb.Collection(collectionName);
        QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                var user = document.ConvertTo<UserRequest>();
                Console.WriteLine($"Fetched User - ID: {user.IdNumber}, Status: {user.Status}");
                users.Add(user);
            }
        }

        return users;
    }



}
