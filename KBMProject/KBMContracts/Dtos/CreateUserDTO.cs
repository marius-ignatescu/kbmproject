﻿namespace KBMContracts.Dtos
{
    public class CreateUserDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
    }
}
