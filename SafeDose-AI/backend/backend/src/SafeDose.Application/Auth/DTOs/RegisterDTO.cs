using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.Auth.DTOs
{
    public class RegisterDTO
    {

        public class PhoneValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(
                object? value,
                ValidationContext validationContext)
            {
                if (value is null)
                    return ValidationResult.Success; // خلي Required مسؤول عن الإلزام

                if (value is not string phoneNumber || string.IsNullOrWhiteSpace(phoneNumber))
                    return new ValidationResult("رقم التليفون غير صحيح");

                try
                {
                    var phoneUtil = PhoneNumberUtil.GetInstance();
                    var parsedNumber = phoneUtil.Parse(phoneNumber, null);

                    return phoneUtil.IsValidNumber(parsedNumber)
                        ? ValidationResult.Success
                        : new ValidationResult(ErrorMessage ?? "رقم التليفون غير صحيح");
                }
                catch (NumberParseException)
                {
                    return new ValidationResult(ErrorMessage ?? "رقم التليفون غير صحيح");
                }
            }
        }





        private string _fullName;
        private string _email;
        private string _userName;
        private string _phoneNumber;


        [MinLength(3),MaxLength(150)]
        public string FullName { get => _fullName; set => _fullName = value.Trim(); }   
        [MinLength(3), MaxLength(80)]
        public string UserName { get => _userName; set => _userName = value.Trim(); }
        [MinLength(1), MaxLength(20)]
        [Required]
        [PhoneValidation(ErrorMessage = "رقم التليفون غير صحيح")]
        public string PhoneNumber { get => _phoneNumber; set => _phoneNumber = value.Trim(); }
        [EmailAddress , MaxLength(150)]
        public string Email { get => _email; set => _email = value.Trim(); }

        [MaxLength(50)]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public bool TermsAndConditions { get; set; }

    }
}
