using System;
using Google.Cloud.Firestore;

namespace CommUnityWeb.Models
{
    [FirestoreData]
    public class UserRequest
    {
        [FirestoreProperty("idNumber")]
        public string IdNumber { get; set; }

        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("surname")]
        public string Surname { get; set; }

        [FirestoreProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [FirestoreProperty("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [FirestoreProperty("age")]
        public int Age { get; set; }

        [FirestoreProperty("dateSubmitted")]
        public DateTime DateSubmitted { get; set; }

        [FirestoreProperty("status")]
        public string Status { get; set; }

        [FirestoreProperty("fcmToken")]
        public string FcmToken { get; set; }


        // Property to get DateOfBirth in local time
        public DateTime LocalDateOfBirth
        {
            get { return DateOfBirth.AddHours(5); }
        }

        // Property to get DateOfBirth in yyyy-MM-dd format
        public string FormattedDateOfBirth
        {
            get { return LocalDateOfBirth.ToString("yyyy-MM-dd"); }
        }
    }
}
