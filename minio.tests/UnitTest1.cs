using System;
using System.Threading.Tasks;
using Minio;
using NUnit.Framework;

namespace minio.tests
{
    public class Tests
    {
        const string Endpoint = "127.0.0.1:9000";
        const string AccessKey = "AKIAIOSFODNN7EXAMPLE";
        const string SecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
        
        [Test]
        public async Task CreateAndListBuckets()
        {
            var minio = new MinioClient(Endpoint, AccessKey, SecretKey)/*.WithSSL()*/;
            
            var listBucketsResult = await minio.ListBucketsAsync();

            for (var i = 0; i < 10; i++)
            {
                var bucketName = "bucket" + i;

                if (await minio.BucketExistsAsync(bucketName) == false)
                {
                    await minio.MakeBucketAsync(bucketName);
                }
            }
            
            foreach (var bucket in listBucketsResult.Buckets)
            {
                Console.WriteLine(bucket.Name + " " + bucket.CreationDateDateTime);
            }
        }
        
        [Test]
        public async Task PutObject()
        {
            var minio = new MinioClient(Endpoint, AccessKey, SecretKey)/*.WithSSL()*/;
            
            var bucketName = "bucket-with-objects";

            if (await minio.BucketExistsAsync(bucketName) == false)
            {
                await minio.MakeBucketAsync(bucketName);
            }

            await minio.PutObjectAsync(bucketName, "UnitTest1.cs", "../../../UnitTest1.cs");
        }
    }
}