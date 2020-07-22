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

        public void UpdateVideo(string video, int videoId, int UserId)
        {
              
            {
                var checkVid1 = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == UserId && w.VideoId == videoId).FirstOrDefault();
                if (checkVid1 != null)
                {
                    checkVid1.Duration = Convert.ToDecimal(video);
                    _dbContext.SaveChanges();

                    var checkVidCount = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == UserId && w.VideoId == videoId).Count();
                    if (checkVidCount > 1)
                    {
                        var checkVid2 = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == UserId && w.VideoId == videoId).FirstOrDefault();
                        _dbContext.tbl_AttendentTestVideos.Remove(checkVid2);
                        _dbContext.SaveChanges();

                    }
                }
                else
                {
                    checkVid1 = new AttendentTestVideo
                    {
                        VideoId = videoId,
                        UserId = UserId,
                        Duration = Convert.ToDecimal(video)
                    };

                    _dbContext.tbl_AttendentTestVideos.Add(checkVid1);
                    _dbContext.SaveChanges();
                }
            }
        }

        public decimal GetVideo(List<AttendentTestVideo> attendantVideoList,  int videoId, int UserId)
        {
            decimal duration = 0;
            var checkVideo = attendantVideoList.Where(w => w.UserId == UserId && w.VideoId == videoId).FirstOrDefault();
            if (checkVideo!=null)
            {
                duration = checkVideo.Duration;
            }
            return duration;
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
