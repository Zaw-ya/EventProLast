namespace EventPro.DAL.Dto
{
    public class UserDto
    {
        public int UserId { get; set; }
        public int? CityId { get; set; }
        public int? Role { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PrimaryContactNo { get; set; }
    }
}
