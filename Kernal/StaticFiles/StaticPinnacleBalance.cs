namespace EventPro.Kernal.StaticFiles
{
    public static class StaticPinnacleBalance
    {
        public static double LowestBinnacleBalanceForNotificationAlert = 3300;

        public static string EventProNotificationEmail = "EventPro96@gmail.com";

        public static string EventProNotificationPassword = "lgmi xnph flra exia";

        public static string EmailAlertMessage(string balance, string employeeName)
        {
            return "<html>\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n       <style>\r\n        .card {\r\n         border: 2px solid #ccc;\r\n            border-radius: 5px;\r\n            box-shadow: 2px 2px 5px #ccc;\r\n            padding: 20px;\r\n              margin: auto;\r\n  width: 50%;  \r\n            display: inline-block;\r\n        }\r\n        .card h3 {\r\n            margin-top: 0;\r\n        }\r\n        .card li { font-size:20px; color:red;       }\r\n        .card p {\r\n  font-size:25px;   \r\n         margin-bottom: 0;\r\n        }\r\n    </style>\r\n</head>\r\n<body >\r\n   <div class=\"content\">\r\n       <div class=\"card\">\r\n       <h1 style=\"color: red; text-size:30px; \">Important Issue!</h1>\r\n    <p>Dear " + employeeName + ",</p>\r\n    <p>We have discovered an important issue that requires your immediate attention. Please be aware of the following information:</p>\r\n    <ul>\r\n        <li  >Current Pinnacle Balance is " + balance + " INR</li>\r\n   </ul>\r\n   <p>Thank you for your attention to this matter.</p>\r\n    <p>Best regards,</p>\r\n    <p>[EventPro technical Team]</p>\r\n </div> \r\n   </div> \r\n   </body>\r\n</html>";
        }
    }

}
