using Google.Cloud.Firestore;

namespace CommUnityWeb.Models
{
    [FirestoreData]
    public class UserLoginModel
    {
        [FirestoreProperty("Email")]
        public string Email { get; set; }
        [FirestoreProperty("Password")]
        public string Password { get; set; }
    }
}
