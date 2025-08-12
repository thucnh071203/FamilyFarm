using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public static class EmailTemplateHelper
    {
        public static string EmailConfirm(string fullname, string content)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Account Updated</title>
                    <style>
                        /* Reset CSS */
                        body, html {{
                            margin: 0;
                            padding: 0;
                            font-family: Arial, sans-serif;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            padding: 20px;
                            background-color: #f9f9f9;
                            border-radius: 10px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                        }}
                        .container h1 {{
                            text-align: center;
                            margin: 0;
                            font-size: 32px;
                            font-weight: 700;
                            letter-spacing: -1px;
                            line-height: 48px;
                        }}
                        .content {{
                            background-color: #fff;
                            padding: 20px;
                            border-radius: 5px;
                            margin-top: 20px;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                        }}
                        .footer p {{
                            color: #888;
                        }}
                        .link-confirm {{
                            display: inline-block;
                            background-color: #007bff;
                            color: white;
                            padding: 10px 20px;
                            text-align: center;
                            border-radius: 5px;
                            text-decoration: none;
                        }}
                        .otp-box {{
                            font-size: 24px;
                            font-weight: bold;
                            color: #ffffff;
                            background-color: #007bff;
                            padding: 10px 20px;
                            border-radius: 8px;
                            display: inline-block;
                            margin: 10px 0;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Confirm Your Email Address</h1>
                        <div class='content'>
                            <p>Hello {fullname},</p>
                            <p>Thank you for being a valued member of our community.</p>    
                            {content}
                            <p>If you have any questions or require further assistance, please do not hesitate to contact our support team.</p>
                            <p>Best regards,</p>
                            <p>Family Farm</p>                            
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 familyfarm0425@gmail.com. Family Farm.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public static string EmailRegister(string fullname, string content)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Account Updated</title>
                    <style>
                        /* Reset CSS */
                        body, html {{
                            margin: 0;
                            padding: 0;
                            font-family: Arial, sans-serif;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            padding: 20px;
                            background-color: #f9f9f9;
                            border-radius: 10px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                        }}
                        .container h1 {{
                            text-align: center;
                            margin: 0;
                            font-size: 32px;
                            font-weight: 700;
                            letter-spacing: -1px;
                            line-height: 48px;
                        }}
                        .content {{
                            background-color: #fff;
                            padding: 20px;
                            border-radius: 5px;
                            margin-top: 20px;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                        }}
                        .footer p {{
                            color: #888;
                        }}
                        .link-confirm {{
                            display: inline-block;
                            background-color: #007bff;
                            color: white;
                            padding: 10px 20px;
                            text-align: center;
                            border-radius: 5px;
                            text-decoration: none;
                        }}
                        .otp-box {{
                            font-size: 24px;
                            font-weight: bold;
                            color: #ffffff;
                            background-color: #007bff;
                            padding: 10px 20px;
                            border-radius: 8px;
                            display: inline-block;
                            margin: 10px 0;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Register</h1>
                        <div class='content'>
                            <p>Hello {fullname},</p>
                            <p>Thank you for being a valued member of our community.</p>    
                            {content}
                            <p>If you have any questions or require further assistance, please do not hesitate to contact our support team.</p>
                            <p>Best regards,</p>
                            <p>Family Farm</p>                            
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 familyfarm0425@gmail.com. Family Farm.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}
