using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Telematics.Models
{

    public class ProfileViewModel
    {
        public ApplicationUser user;
        public Companie company;
    }

    public class ScoreViewModel
    {
        public List<ScoreList> scores;
        public int gradeA;
        public int gradeB;
        public int gradeC;
    }

    public class TrackViewModel
    {
        public List<Drivers_GPSData> drivergps;
    }

    public class DetailsViewModel
    {
        public List<Trips> trips;
        public string driver;
        public double getTotalScore()
        {
            var totalScore = 0.0;
            foreach(Trips t in trips)
            {
                totalScore += t.score;
            }
            return Math.Round(totalScore / trips.Count, 2);
        }
        public double getTotalHardAcceleration()
        {
            var totalHardAcceleration = 0.0;
            foreach(Trips t in trips)
            {
                totalHardAcceleration += t.hardAccelerations;
            }
            return totalHardAcceleration;
        }
        public double getTotalHardStop()
        {
            var totalHardStop = 0.0;
            foreach(Trips t in trips)
            {
                totalHardStop += t.hardStops;
            }
            return totalHardStop;
        }
        public double getTotalHardTurn()
        {
            var totalHardTurn = 0.0;
            foreach(Trips t in trips)
            {
                totalHardTurn += t.hardTurns;
            }
            return totalHardTurn;
        }
        public double getTotalDistance()
        {
            var totalDistance = 0.0;
            foreach(Trips t in trips)
            {
                totalDistance += t.distance;
            }
            return Math.Round(totalDistance / 1000, 2);
        }
        public double getTotalDuration()
        {
            var totalDuration = 0.0;
            foreach(Trips t in trips)
            {
                totalDuration += t.duration;
            }
            return Math.Round(totalDuration / 60, 2);
        }
        public double getTotalSpeeding()
        {
            var totalSpeeding = 0.0;
            foreach(Trips t in trips)
            {
                if(t.maxSpeed > 80)
                totalSpeeding += 1;
            }
            return Math.Round(totalSpeeding, 2);
        }
        public double getTotalPhoneUsage()
        {
            var totalPhoneUsage = 0.0;
            foreach(Trips t in trips)
            {
                totalPhoneUsage += t.phoneUsage;
            }
            return Math.Round(totalPhoneUsage, 2);
        }
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Access Right")]
        public string AccessRole { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class VehicleRegisterViewModel
    {
        public int id { get; set; }

        [Required]
        [Display(Name = "License Plate Number")]
        public string LicensePlateNumber { get; set; }
        
        [Display(Name = "Brand")]
        public string Brand { get; set; }
        
        [Display(Name = "Model")]
        public string VehicleModel { get; set; }
        
        [Display(Name = "OBD Device Number")]
        public string DeviceNo { get; set; }
    }

    public class Vehicle
    {
        public int id { get; set; }
        public string lic_no { get; set; }
        public string brand { get; set; }
        public string v_model { get; set; }
        public string device_no { get; set; }
    }

    public class Vehicles
    {
        public IList<Telematics.Models.Vehicle> vehicle_list { get; set; }
    }

    public class DriverScore
    {
        public int id { get; set; }
        public int driver_id { get; set; }
        public int driver_score { get; set; }
    }

    public class UserWithRoles
    {
        public IList<Telematics.Models.ApplicationUser> users { get; set; }
        public IList<string> roles { get; set; }
    }

    public class EditViewModel
    {
        public string Id { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        
        [Required]
        [Display(Name = "Access Right")]
        public string AccessRole { get; set; }
    }
}
