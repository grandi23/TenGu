﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Tuhu.YeWu.TenGu.Models
{

    #region Models

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "当前密码")]
        public string OldPassword { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "再次输入新密码")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }

        public string ValidatedCode { get; set; }
    }


    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string PasswordQuestion { get; set; }

        [Required]
        [Display(Name = "Answer")]
        public string PasswordAnswer { get; set; }

    }
    #endregion

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

//    public interface IMembershipService
//    {
//        int MinPasswordLength { get; }

//        bool ValidateUser(string userName, string password);
//        MembershipCreateStatus CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer);
//        bool ChangePassword(string userName, string oldPassword, string newPassword);
//    }

//    public class AccountMembershipService : IMembershipService
//    {
//        private readonly MembershipProvider _provider;

//        public AccountMembershipService()
//            : this(null)
//        {
//        }

//        public AccountMembershipService(MembershipProvider provider)
//        {
//            _provider = provider ?? Membership.Provider;
//        }

//        public int MinPasswordLength
//        {
//            get
//            {
//                return _provider.MinRequiredPasswordLength;
//            }
//        }

////        public bool ValidateUser(string userName, string password)
////        {
////            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
////            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

////            if (_provider.ValidateUser(userName, password))
////            {
////                Guid registeredUserId = (Guid)Membership.GetUser(userName).ProviderUserKey;
////                UserProfilePlus upp = UserProfilePlus.Get(registeredUserId);

////                if (upp.UserType != UserType.InternalUser)
////                {
////                    return false;
////                }
////                else
////                    return true;
////            }  
////            else
////                return false;
/////*            {
////                try
////                {
////                    MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline );
////                    if (currentUser.Email != UserType.InternalUser)
////                    {
////                        FormsAuthentication.SignOut();
////                    }
////                    else return true;

////                }
////                catch (ArgumentException)
////                {
////                    return false;
////                }
////            }
////            else 
////            return false;*/
////        }

//        public MembershipCreateStatus CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer)
//        {
//            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
//            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
//            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");
//            if (String.IsNullOrEmpty(passwordQuestion)) throw new ArgumentException("Value cannot be null or empty.", "passwordQuestion");
//            if (String.IsNullOrEmpty(passwordAnswer)) throw new ArgumentException("Value cannot be null or empty.", "passwordAnswer");

//            MembershipCreateStatus status;
//            _provider.CreateUser(userName, password, email, passwordQuestion, passwordAnswer, false, null, out status);
//            return status;
//        }

//        public bool ChangePassword(string userName, string oldPassword, string newPassword)
//        {
//            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
//            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
//            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

//            // The underlying ChangePassword() will throw an exception rather
//            // than return false in certain failure scenarios.
//            try
//            {
//                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
//                return currentUser.ChangePassword(oldPassword, newPassword);
//            }
//            catch (ArgumentException)
//            {
//                return false;
//            }
//            catch (MembershipPasswordException)
//            {
//                return false;
//            }
//        }
//    }

    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
    #endregion

    #region Validation
    public static class AccountValidation
    {
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
    {
        private const string _defaultErrorMessage = "'{0}' must be at least {1} characters long.";
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]{
                new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
            };
        }
    }
    #endregion

}