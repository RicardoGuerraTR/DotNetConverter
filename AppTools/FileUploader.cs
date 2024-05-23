using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileUploader
{
    internal class FileUploader
    {
        readonly string appMode = Environment.GetEnvironmentVariable("APP_MODE") ?? "LOCAL";
        readonly string awsProfile = "tr-integrationpoint-preprod";
        readonly string s3BucketName = "a205822-gtm-generative-ai-assignment";
        readonly string keyPrefix = "6118944/";

        public async Task StartUploading()
        {
            if (appMode == "LOCAL")
            {
                ReadFilesFromFolder(@"D:\dev\ogt\ogt-legacy_ipapi");
            }
            else if (appMode == "AWS")
            {
                await ReadFilesAndPushToS3(@"D:\", awsProfile, s3BucketName, keyPrefix);
            }
            else
            {
                Console.WriteLine("Invalid app mode. Use 'LOCAL' or 'AWS'.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void ReadFilesFromFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    try
                    {
                        Console.WriteLine($"Reading file: {file}");
                        // Add your logic to read the file contents here
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error reading file: {file}||| {e.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Folder {folderPath} does not exist.");
            }
        }

        async static Task ReadFilesAndPushToS3(string folderPath, string awsProfile, string bucketName, string keyPrefix)
        {
            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                AWSConfigs.LoggingConfig.LogMetrics = true;
                AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
                AWSConfigs.LoggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
                AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Console;
                var credentialProfileStoreChain = new CredentialProfileStoreChain();
                AWSCredentials awsCredentials;

                if (credentialProfileStoreChain.TryGetAWSCredentials(awsProfile, out awsCredentials))
                {
                    using (var client = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.USEast1))
                    {
                        foreach (string file in files)
                        {
                            Console.WriteLine($"Uploading file: {file}");
                            var fileTransferUtility = new TransferUtility(client);
                            try
                            {
                                string key = Path.Combine(keyPrefix, Path.GetFileName(file));
                                await fileTransferUtility.UploadAsync(file, bucketName, key);
                                Console.WriteLine($"Uploaded correctly: {file}");
                            }
                            catch (AmazonS3Exception e)
                            {
                                Console.WriteLine($"Error uploading file: {file}||| {e.Message}");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Folder {folderPath} does not exist.");
            }
        }
    }
}