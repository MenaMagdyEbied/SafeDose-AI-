namespace SafeDose.Application.Auth.DTOs
{
    public class AuthModelDTO
    {
        public string Message { get; set; }    

        public bool IsAuthenticated { get; set; }   

        public string UserName { get; set; }
        public string Email { get; set; }   

        public DateTime ExpiresOn { get; set; }
        public string Token { get;  set; }
    }
}
