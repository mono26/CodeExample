using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.IO;
using System;
using Amazon.S3.Util;
using System.Collections.Generic;
using Amazon.CognitoIdentity;
using Amazon;
using UnityEngine.Events;

public class AWSInterface : MonoBehaviour
{
    public string IdentityPoolId = "";
    public string CognitoIdentityRegion = RegionEndpoint.USEast1.SystemName;
    private RegionEndpoint _CognitoIdentityRegion
    {
        get { return RegionEndpoint.GetBySystemName(CognitoIdentityRegion); }
    }
    public string S3Region = RegionEndpoint.USEast1.SystemName;
    private RegionEndpoint _S3Region
    {
        get { return RegionEndpoint.GetBySystemName(S3Region); }
    }
    public string S3BucketName = null;

    public static AWSInterface Instance {
        get
        {
            if(_instance == null)
            {
                var go =  new GameObject();
                _instance = go.AddComponent<AWSInterface>();
            }
            return _instance;
        }
    }
    private static AWSInterface _instance;

    
    void Awake() 
    {
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);

        // Fixes: "InvalidOperationException: Cannot override system-specified headers"
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;
        AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
        AWSConfigs.LoggingConfig.LogMetrics = true;
        AWSConfigs.CorrectForClockSkew = true;
    }

    #region private members

    private IAmazonS3 _s3Client;
    private AWSCredentials _credentials;

    private AWSCredentials Credentials
    {
        get
        {
            if (_credentials == null)
            {
                _credentials = new CognitoAWSCredentials(IdentityPoolId, _CognitoIdentityRegion);
            }

            return _credentials;
        }
    }

    private IAmazonS3 Client
    {
        get
        {
            if (_s3Client == null)
            {
                _s3Client = new AmazonS3Client(Credentials, _S3Region);
            }
            //test comment
            return _s3Client;
        }
    }

    #endregion

    #region Get Bucket List
    /// <summary>
    /// Example method to Demostrate GetBucketList
    /// </summary>
    public void GetBucketList()
    {
        Debug.Log("Fetching all the Buckets");
        Client.ListBucketsAsync(new ListBucketsRequest(), (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                Debug.Log("Got Response Printing now");
                responseObject.Response.Buckets.ForEach((s3b) =>
                {
                    Debug.Log(string.Format("bucket = {0}, created date = {1} \n", s3b.BucketName, s3b.CreationDate));
                });
            }
            else
            {
                Debug.Log("Got Exception");
            }
        });
    }

    #endregion

    /// <summary>
    /// Get Object from S3 Bucket
    /// </summary>
    public void GetObject(string imageKey, UnityAction<Texture2D> successCallback, UnityAction failCallback)
    {
        Debug.Log(string.Format("fetching {0} from bucket {1}", imageKey, S3BucketName));
        try
        {
            Client.GetObjectAsync(S3BucketName, imageKey, (responseObj) =>
            {
                byte[] data = null;
                var response = responseObj.Response;
                Stream input = response.ResponseStream;
                Texture2D image = null;

                if (response.ResponseStream != null)
                {
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        data = ms.ToArray();
                    }

                    image = bytesToTexture2D(data);

                    successCallback(image);
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.Log("Unhandled Exception while getting the object");
            Debug.Log(string.Format(" received error {0}", e.Message));
            failCallback();
        }
    }

    public Texture2D bytesToTexture2D(byte[] imageBytes)
    {
        Texture2D tex = new Texture2D(512, 512);
        tex.LoadImage(imageBytes);
        return tex;
    }

    /// <summary>
    /// Post Object to S3 Bucket. 
    /// </summary>
    public void PostObject(string path, UnityAction<string> successCallback, UnityAction<string> failCallback)
    {
        Debug.Log("Retrieving the file");
        string fileName = GetFileHelper(path);
        Debug.Log("fileName: " + fileName);
            
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        Debug.Log("Creating request object");

        // Changed from the sample to include region to fix:
        // HttpErrorResponseException 
        var request = new PostObjectRequest() {
            Bucket = S3BucketName,
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.Private,
            Region = RegionEndpoint.USEast1
        };

        request.Headers.ContentType = "image/png";

        Debug.Log("Making HTTP post call");

        try
        {
            Client.PostObjectAsync(request, (responseObj) =>
            {
                Debug.Log("request.Path: " + request.Path);
                Debug.Log("responseObj.Request.Path: " + responseObj.Request.Path);

                if (responseObj.Exception == null)
                {
                    Debug.Log(string.Format("object {0} posted to bucket {1}", responseObj.Request.Key, responseObj.Request.Bucket));
                    successCallback(path);
                }
                else
                {
                    Debug.Log("Exception while posting the result object");
                    // Changed from sample so we can see actual error and not null pointer exception
                    Debug.Log(string.Format(" receieved error {0}", responseObj.Exception.ToString()));
                    failCallback(path);
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.Log("Unhandled Exception while posting the result object");
            Debug.Log(string.Format(" received error {0}", e.Message));
        }
    }

    /// <summary>
    /// Get Objects from S3 Bucket
    /// </summary>
    public void GetObjects()
    {
        Debug.Log("Fetching all the Objects from " + S3BucketName);

        var request = new ListObjectsRequest()
        {
            BucketName = S3BucketName
        };

        try
        {
            Client.ListObjectsAsync(request, (responseObject) =>
            {
                Debug.Log("\n");
                if (responseObject.Exception == null)
                {
                    Debug.Log("Got Response \nPrinting now \n");
                    responseObject.Response.S3Objects.ForEach((o) =>
                    {
                        Debug.Log(string.Format("{0}\n", o.Key));
                    });
                }
                else
                {
                    Debug.Log("Got Exception \n");
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.Log("Unhandled Exception while getting the objects");
            Debug.Log(string.Format(" received error {0}", e.Message));
        }
    }

    /// <summary>
    /// Delete Objects in S3 Bucket
    /// </summary>
    public void DeleteObject(string path)
    {
        Debug.Log(string.Format("deleting {0} from bucket {1}", path, S3BucketName));
        List<KeyVersion> objects = new List<KeyVersion>();
        objects.Add(new KeyVersion()
        {
            Key = path
        });

        var request = new DeleteObjectsRequest()
        {
            BucketName = S3BucketName,
            Objects = objects
        };

        Client.DeleteObjectsAsync(request, (responseObj) =>
        {
            Debug.Log("\n");
            if (responseObj.Exception == null)
            {
                Debug.Log("Got Response \n \n");

                Debug.Log(string.Format("deleted objects \n"));

                responseObj.Response.DeletedObjects.ForEach((dObj) =>
                {
                    Debug.Log(dObj.Key);
                });
            }
            else
            {
                Debug.Log("Got Exception \n");
            }
        });
    }


    #region helper methods

    private string GetFileHelper(string path)
    {
        var fileName = Path.GetFileName(path);
        Debug.Log("path: " + path);
        
        Debug.Log("File exists in pth: " + File.Exists(path));
        if (!File.Exists(path))
        //if (!File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + fileName))
        {
            var streamReader = File.CreateText(Application.persistentDataPath + Path.DirectorySeparatorChar + fileName);
            streamReader.WriteLine("This is a sample s3 file uploaded from unity s3 sample");
            streamReader.Close();
        }
        return fileName;
    }

    private string GetPostPolicy(string bucketName, string key, string contentType)
    {
        bucketName = bucketName.Trim();

        key = key.Trim();
        // uploadFileName cannot start with /
        if (!string.IsNullOrEmpty(key) && key[0] == '/')
        {
            throw new ArgumentException("uploadFileName cannot start with / ");
        }

        contentType = contentType.Trim();

        if (string.IsNullOrEmpty(bucketName))
        {
            throw new ArgumentException("bucketName cannot be null or empty. It's required to build post policy");
        }
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("uploadFileName cannot be null or empty. It's required to build post policy");
        }
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentException("contentType cannot be null or empty. It's required to build post policy");
        }

        string policyString = null;
        int position = key.LastIndexOf('/');
        if (position == -1)
        {
            policyString = "{\"expiration\": \"" + DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ") + "\",\"conditions\": [{\"bucket\": \"" +
                bucketName + "\"},[\"starts-with\", \"$key\", \"" + "\"],{\"acl\": \"private\"},[\"eq\", \"$Content-Type\", " + "\"" + contentType + "\"" + "]]}";
        }
        else
        {
            policyString = "{\"expiration\": \"" + DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ") + "\",\"conditions\": [{\"bucket\": \"" +
                bucketName + "\"},[\"starts-with\", \"$key\", \"" + key.Substring(0, position) + "/\"],{\"acl\": \"private\"},[\"eq\", \"$Content-Type\", " + "\"" + contentType + "\"" + "]]}";
        }

        return policyString;
    }
    #endregion
}
