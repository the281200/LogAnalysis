using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;


namespace WebModels
{


    [Table("webpages_Roles")]
    public class WebRole
    {
        private string _roleName = string.Empty;
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRoleName")]
        [Display(ResourceType = typeof(AccountResources), Name = "RoleName")]
        public string RoleName
        {
            get { return _roleName.Trim(); }
            set { _roleName = value.Trim(); }
        }
        [Display(ResourceType = typeof(AccountResources), Name = "RoleDescription")]
        public string Description { get; set; }
        [Display(ResourceType = typeof(AccountResources), Name = "Title")]
        public string Title { get; set; }

        public virtual ICollection<AccessAdminSiteRole> AccessAdminSiteRoles { get; set; }

        public virtual ICollection<AccessWebModuleRole> AccessWebModuleRoles { get; set; }
    }

    [Table("UserProfile")]
    public partial class UserProfile
    {
        //public UserProfile()
        //{
        //    this.AccessAdminSites = new HashSet<AccessAdminSite>();
        //    this.AccessWebModules = new HashSet<AccessWebModule>();
        //}
        private string _userName = string.Empty;
        [Key]
        [AllowHtml]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        //[RegularExpression(@"^[a-zA-Z0-9_.-]*$", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "UserNameNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName
        {
            get { return _userName.Trim(); }
            set { _userName = value.Trim(); }
        }
        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredFulllName")]
        [Display(ResourceType = typeof(AccountResources), Name = "FulllName")]

        public string FullName { get; set; }
        [AllowHtml]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "EmailNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }
        [AllowHtml]
        /* [RegularExpression(@"^(84|0[3|5|7|8|9])+([0-9]{8})\b$", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "MobileNotValid")]*/
        [Display(ResourceType = typeof(AccountResources), Name = "Mobile")]
        public string Mobile { get; set; }
        [AllowHtml]
        [Display(ResourceType = typeof(AccountResources), Name = "Avatar")]
        public string Avatar { get; set; }
        [AllowHtml]
        [Display(ResourceType = typeof(AccountResources), Name = "Position")]
        public string Position { get; set; }
        [AllowHtml]
        [Display(Name = "Loại người dùng")]
        public int? Type { get; set; }
        [AllowHtml]
        [Display(Name = "Hộ chiếu")]
        public string Passport { get; set; }
        [AllowHtml]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
        [AllowHtml]
        [Display(Name = "Thông tin")]
        public string Infomation { get; set; }
        [AllowHtml]
        [Display(Name = "Invite Code")]
        public string InviteCode { get; set; }
        [AllowHtml]
        [Display(Name = "Is Active")]
        public bool? IsActive { get; set; }
        [AllowHtml]
        [Display(Name = "Giới tính")]
        public int? Gender { get; set; }
        [AllowHtml]
        [Display(Name = "Người tạo")]
        public int? CreatedBy { get; set; }
        [AllowHtml]
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
        [AllowHtml]
        [Display(Name = "Người sửa")]
        public int? ModifiedBy { get; set; }
        [AllowHtml]
        [Display(Name = "Ngày sửa")]
        public DateTime? ModifiedAt { get; set; }
        [AllowHtml]
        [Display(Name = "Khách hàng kích hoạt tài khoản")]
        public bool? IsCustomerActive { get; set; }
        [AllowHtml]
        [Display(Name = "Trạng thái gửi mail và kích hoạt tài khoản")]
        public int? CustomerActive { get; set; }
        [AllowHtml]
        public Boolean? IsReadNotification { get; set; }
        [AllowHtml]
        public DateTime? DateNotification { get; set; }
        [AllowHtml]
        public int? UnReadNotiCount { get; set; }
        [Display(Name = "Người đại diện theo pháp luật")]
        public string LegalRepresentative { get; set; }
        [AllowHtml]
        [Display(Name = "Chức vụ người đại diện")]
        public string RepresentativePosition { get; set; }
        [AllowHtml]
        [Display(Name = "Số đăng ký kinh doanh")]
        public string BusinessRegistrationNumber { get; set; }
        [AllowHtml]
        [Display(Name = "Người đại diện theo ủy quyền")]
        public string AuthorizedPerson { get; set; }
        [AllowHtml]
        [Display(Name = "Số ủy quyền")]
        public string AuthorizationNumber { get; set; }
    }


    [Table("webpages_UsersInRoles")]
    public class UserInRole
    {
        [Key, Column(Order = 0)]
        public int UserId { get; set; }

        [Key, Column(Order = 1)]
        public int RoleId { get; set; }
    }
    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu cũ")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
      
        public string ConfirmPassword { get; set; }
    }
    public class LoginCustomerModel
    {
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "EmailNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPassword")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPassword")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        private string _userName = string.Empty;
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        [RegularExpression(@"^[a-zA-Z0-9@_.-]*$", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "UserNameNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName
        {
            get { return _userName.Trim(); }
            set { _userName = value.Trim(); }
        }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPassword")]
        [StringLength(100, ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "PasswordLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "ConfirmPassword")]
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredConfirmPassword")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "PasswordNotMatch")]
        public string ConfirmPassword { get; set; }

        public int? PartnerID { get; set; }
        public UserProfile UserProfile { get; set; }
    }

    public class CustomerModel
    {
        private string _userName = string.Empty;
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        //[RegularExpression(@"^[a-zA-Z0-9@_.-]*$", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "UserNameNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName
        {
            get { return _userName.Trim(); }
            set { _userName = value.Trim(); }
        }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPassword")]
        //[StringLength(100, ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "PasswordLength", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(ResourceType = typeof(AccountResources), Name = "Password")]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(ResourceType = typeof(AccountResources), Name = "ConfirmPassword")]
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredConfirmPassword")]
        //[Compare("Password", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "PasswordNotMatch")]
        //public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredFulllName")]
        [Display(ResourceType = typeof(AccountResources), Name = "FulllName")]
        public string FullName { get; set; }

        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "EmailNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }

        /*[RegularExpression(@"^(84|0[3|5|7|8|9])+([0-9]{8})\b$", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "MobileNotValid")]*/
        [Display(ResourceType = typeof(AccountResources), Name = "Mobile")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Mobile { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "Avatar")]
        public string Avatar { get; set; }

        [Display(Name = "Loại người dùng")]
        public int Type { get; set; }

        [Display(Name = "Giới tính")]
        public int? Gender { get; set; }

        [Display(Name = "Hộ chiếu")]
        public string Passport { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Thông tin")]
        public string Infomation { get; set; }

        [Display(Name = "Invite Code")]
        public string InviteCode { get; set; }

        [Display(Name = "Người đại diện theo pháp luật")]
        public string LegalRepresentative { get; set; }

        [Display(Name = "Chức vụ người đại diện")]
        public string RepresentativePosition { get; set; }

        [Display(Name = "Số đăng ký kinh doanh")]
        public string BusinessRegistrationNumber { get; set; }

        [Display(Name = "Người đại diện theo ủy quyền")]
        public string AuthorizedPerson { get; set; }

        [Display(Name = "Số ủy quyền")]
        public string AuthorizationNumber { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
    public class ForgotPasswordModel
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Token { get; set; }
    }

    [Table("ForgotPassWord")]
    public class ForgotPassWord
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string RandomString { get; set; }
        public bool Used { get; set; }
        public DateTime ExpiredTime { get; set; }
    }

    public class MailForgotModel
    {
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "EmailNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]

        public string Email { get; set; }
    }

}
