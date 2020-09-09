using Microsoft.Extensions.Options;
using EmployeeTest.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace EmployeeTest.Utility
{
    public class VideoUtility
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        public VideoUtility(IOptions<Appsettings> appSettings, DBContext dbContext)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
        }

        public void UpdateVideo(string video, int videoId, int UserId, bool isCompleted)
        {
            var checkVid1 = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == UserId && w.VideoId == videoId).FirstOrDefault();
            if (checkVid1 != null)
            {
                if (Convert.ToDecimal(video) > checkVid1.Duration)
                {
                    checkVid1.Duration = Convert.ToDecimal(video);


                }
                if (isCompleted)
                {
                    checkVid1.IsCompleted = true;
                }
                _dbContext.SaveChanges();

           
            }
            else
            {
                checkVid1 = new AttendentTestVideo
                {
                    VideoId = videoId,
                    UserId = UserId,
                    Duration = Convert.ToDecimal(video),
                    IsCompleted = false
                };

                _dbContext.tbl_AttendentTestVideos.Add(checkVid1);
                _dbContext.SaveChanges();
            }
        }

        public VideoTimeDuration GetVideo(List<AttendentTestVideo> attendantVideoList, int videoId, int UserId)
        {
            decimal duration = 0;
            bool IsCompleted = false;
            var checkVideo = attendantVideoList.Where(w => w.UserId == UserId && w.VideoId == videoId).FirstOrDefault();
            if (checkVideo != null)
            {
                duration = checkVideo.Duration;
                IsCompleted = checkVideo.IsCompleted ?? false;
            }
            return new VideoTimeDuration() { Duration = duration, Completed = IsCompleted };
        }

        public decimal GetVideoDuration(List<Testvideo> attendantVideoList, int videoId)
        {
            decimal duration = 0;
            var checkVideo = attendantVideoList.Where(w => w.Id == videoId).FirstOrDefault();
            if (checkVideo != null)
            {
                duration = checkVideo.Duration;
            }
            return duration;
        }

    }
}
