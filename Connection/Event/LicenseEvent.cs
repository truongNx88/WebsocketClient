/**
* LicenseEvent.cs
* Author: Nguyen Xuan Truong
* Created at: 03/05/2022
*/

namespace Event.LicenseEvent {
    class LicenseEvent {
        private string license;

        private string cropsUrl;

        private string imageUrl;

        private string createdTime;

        public LicenseEvent(string license, string cropsUrl, string imageUrl, string createdTime) {
            this.license = license;
            this.cropsUrl = cropsUrl;
            this.imageUrl = imageUrl;
            this.createdTime = createdTime;
        }

    }
}