using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;
using System;

[Serializable]
public class ImageData
{
    public string keyValue = null;
    public int likes = 0;
    public string[] comments = new string[0];

    public void AddLike()
    {
        likes++;

        // Mandar a actualizar info de la imagen
    }

    public void AddComment(string comment)
    {
        List<string> currentComments  = new List<string>(comments);
        currentComments.Add(comment);
        comments = currentComments.ToArray();

        // Mandar a actualizar info de la imagen
    }
}

[Serializable]
public class UserImageCollection
{
    public ImageData[] imgCollection = new ImageData[0];

    public void AddImageToCollection(string imageKey) 
    {
        List<ImageData> currentColection  = new List<ImageData>(imgCollection);
        currentColection.Add(new ImageData() 
        {
            keyValue = imageKey,
            likes = 0,
            comments = new string[0]
        });
        imgCollection = currentColection.ToArray();
    }

    public void DeleteImageFromCollection(ImageData image) 
    {
        List<ImageData> currentColection  = new List<ImageData>(imgCollection);
        currentColection.Remove(image);
        imgCollection = currentColection.ToArray();
    }
}

public class ProfilePanel : MonoBehaviour
{
#region Variables
    [SerializeField] Text nameTXT = null;
    [SerializeField] Text userNameTXT =  null;
    [SerializeField] Text descriptionTXT = null;
    [SerializeField] Button profileBTN = null;
    [SerializeField] Image profileIMG = null;
    [SerializeField] Button editDescriptionBTN = null;
    [SerializeField] Button followProfileBTN = null;
    [SerializeField] ProfileDescription descriptionPopup = null;
    [SerializeField] string displayedProfileId = "";
    [SerializeField] LoadingPanel userInfoLoading = null;
    [SerializeField] LoadingPanel userImagesLoading = null;
    [SerializeField] LoadingPanel userProfilePicLoading = null;
    [SerializeField] int maxImageSize = 512;
    [SerializeField] Button addImageBTN = null;
    [SerializeField] PooledScroll imagesContainer = null;
    [SerializeField] string editorImageToUpload = "Queen.jpg";
    [SerializeField] UserImageCollection profileImages = null;
#endregion

    public delegate void OnCollectionUpdated();
    public OnCollectionUpdated CollectionUpdatedEvent;

#region Unity Functions
    void OnEnable()
    {
        if (editDescriptionBTN)
        {
            editDescriptionBTN.onClick.AddListener(descriptionPopup.ToggleDescription);
        }
        else
        {
            Debug.LogError("ProfilePanel is missing editDescription button reference");
        }

        if (followProfileBTN)
        {
            followProfileBTN.onClick.AddListener(FollowProfile);
        }
        else
        {
            Debug.LogError("ProfilePanel is missing followProfile button reference");
        }

        if (profileBTN)
        {
            profileBTN.onClick.AddListener(PickProfileImage);
        }
        else
        {
            Debug.LogError("ProfilePanel is missing profilePicture button reference");
        }

        if (addImageBTN)
        {
            addImageBTN.onClick.AddListener(AddImage);
        }
        else
        {
            Debug.LogError("ProfilePanel is missing addImage button reference");
        }
    }

    void OnDisable()
    {
        if (editDescriptionBTN)
        {
            editDescriptionBTN.onClick.RemoveAllListeners();
        }
        else
        {
            Debug.LogError("ProfilePanel is missing editDescription button reference");
        }

        if (followProfileBTN)
        {
            followProfileBTN.onClick.RemoveAllListeners();
        }
        else
        {
            Debug.LogError("ProfilePanel is missing followProfile button reference");
        }

        if (profileBTN)
        {
            profileBTN.onClick.RemoveAllListeners();
        }
        else
        {
            Debug.LogError("ProfilePanel is missing profilePicture button reference");
        }

        if (addImageBTN)
        {
            addImageBTN.onClick.RemoveAllListeners();
        }
        else
        {
            Debug.LogError("ProfilePanel is missing addImage button reference");
        }
    }
#endregion

#region Custom Functions
    public void Initialize()
    {
        // DisplayPlayerProfile();
    }
 
    public void DisplayPlayerProfile()
    {
        DisplayFollowButton(false);

        DisplayEditButton(true);

        DisplayAddImageButton(true);

        DisplayProfile(PlayFabServices.GetPlayFabId);
    }

    void DisplayFollowButton(bool display)
    {
        if (followProfileBTN)
        {
            followProfileBTN.gameObject.SetActive(display);
        }
        else
        {
            Debug.LogError("ProfilePanel is missing followProfileBTN reference");
        }
    }

    void DisplayEditButton(bool display)
    {
        if (editDescriptionBTN)
        {
            editDescriptionBTN.gameObject.SetActive(display);
        }
        else
        {
            Debug.LogError("ProfilePanel is missing editDescription button reference");
        }
    }

    void DisplayAddImageButton(bool display)
    {
        if (addImageBTN)
        {
            addImageBTN.gameObject.SetActive(display);
        }
        else
        {
            Debug.LogError("ProfilePanel is missing addImageBTN reference");
        }
    }

    void DisplayProfile(string playFabId)
    {
        if (displayedProfileId.Equals(playFabId))
        {
            return;
        }
        
        displayedProfileId = playFabId;

        DisplayProfileInfo(playFabId);

        PlayFabServices.Instance.GetUserData(new GetUserDataParameters()
        {
            userId = playFabId,
            requiredData = new List<string>() {"UserDescription", "UserImages", "UserNameAndLastName"},
            succesCallback =  (getResult) =>
            {
                Debug.Log("Got user data for profile");
                if (nameTXT)
                {
                    if (getResult.Data == null || !getResult.Data.ContainsKey("UserNameAndLastName"))
                    {
                        Debug.Log("No UserNameAndLastName");
                        nameTXT.text = "";
                    }
                    else 
                    {
                        Debug.Log(getResult.Data["UserNameAndLastName"].Value);
                        nameTXT.text = getResult.Data["UserNameAndLastName"].Value;
                    }
                }
                else
                {
                    Debug.LogError("ProfilePanel is missing nameTXT reference");
                }

                DisplayProfileDescription(getResult);

                DisplayProfileImages(getResult);
            },
            errorCallback = () =>
            {
                Debug.LogError("Got error getting data for user profile");
            }
        });
    }

    void DisplayProfileInfo(string playFabId)
    {
        userInfoLoading.StartLoading();
        userInfoLoading.UpdateLabel(string.Empty);

        var profileRequest = new GetPlayerProfileRequest() {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints(){
                ShowAvatarUrl = true,
                ShowDisplayName = true
            }
        };

        PlayFabClientAPI.GetPlayerProfile(profileRequest, (profileResult) => {
            if (userNameTXT != null)
            {
                userNameTXT.text = profileResult.PlayerProfile.DisplayName;
            }
            else
            {
                Debug.LogError("ProfilePanel is missing userName text reference");
            }

            DisplayProfilePicture(profileResult);
        }, PlayFabServices.OnPlayFabError);
    }

    void DisplayProfilePicture(GetPlayerProfileResult profile)
    {
        userProfilePicLoading.StartLoading();
        userProfilePicLoading.UpdateLabel(string.Empty);

        if (profileIMG)
        {
            Debug.Log("profile.PlayerProfile.AvatarUrl: " + profile.PlayerProfile.AvatarUrl);
            if (string.IsNullOrEmpty(profile.PlayerProfile.AvatarUrl) || string.IsNullOrWhiteSpace(profile.PlayerProfile.AvatarUrl))
            {
                userProfilePicLoading.FinishLoading();
                userProfilePicLoading.UpdateLabel(string.Empty);
                return;
            }
            AWSInterface.Instance.GetObject(profile.PlayerProfile.AvatarUrl, (image) => 
            {
                Material material = profileIMG.material;
                if(!material.shader.isSupported) // happens when Standard shader is not included in the build
                {
                    material.shader = Shader.Find( "Legacy Shaders/Diffuse" );
                }

                material.mainTexture = image;
                profileIMG.sprite = null;
                // var profilePic = Sprite.Create(texture, ((RectTransform)profileBTN.transform).rect, new Vector2(0.5f, 0.5f), 1.0f);
                // profileIMG.sprite = profilePic;

                userProfilePicLoading.FinishLoading();
                userProfilePicLoading.UpdateLabel(string.Empty);
            }, () => {
                Debug.LogError("Error trying to get bucket image from bucket");
                userProfilePicLoading.FinishLoading();
                userProfilePicLoading.UpdateLabel("Error trying to get bucket image from bucket");
            });
        }
        else
        {
            Debug.LogError("ProfilePanel is missing profilePicture button reference");
            userProfilePicLoading.FinishLoading();
            userProfilePicLoading.UpdateLabel("ProfilePanel is missing profilePicture button reference");
        }
    }

    void DisplayProfileDescription(GetUserDataResult userDataResult)
    {
        //DisplayProfileImages(userDataResult);

        if (userDataResult.Data == null || !userDataResult.Data.ContainsKey("UserDescription"))
        {
            Debug.Log("No UserDescription");
            return;
            // descriptionTXT.text = "";
        }

        Debug.Log(userDataResult.Data["UserDescription"].Value);
        descriptionTXT.text = userDataResult.Data["UserDescription"].Value;

        userInfoLoading.FinishLoading();
        userInfoLoading.UpdateLabel(string.Empty);
    }

    void DisplayProfileImages(GetUserDataResult userDataResult)
    {
        if (userDataResult.Data == null || !userDataResult.Data.ContainsKey("UserImages"))
        {
            Debug.Log("No UserImages");
            return;
        }

        UserImageCollection images = JsonUtility.FromJson<UserImageCollection>(userDataResult.Data["UserImages"].Value);
        profileImages = images;
        imagesContainer.Initialize(new ScrollData()
        {
            userId = displayedProfileId,
            imgsData = images
        });

        userImagesLoading.FinishLoading();
        userImagesLoading.UpdateLabel(string.Empty);
    }

    void DeleteImage(ImageData image)
    {
        profileImages.DeleteImageFromCollection(image);
    }

    public void DisplayUserProfile(string _playFabId)
    {
        DisplayFollowButton(true);

        DisplayEditButton(false);

        DisplayAddImageButton(false);

        DisplayProfile(_playFabId);
    }

    void PickProfileImage()
    {
        StartCoroutine(PickProfileImageProcess());
    }

    IEnumerator PickProfileImageProcess()
    {
        string pickedImagePath = "";
        bool hasPicked = false;
        bool hasCanceled = false;

#if UNITY_EDITOR
        Debug.LogError("Executing from Unity editor");
        pickedImagePath = Application.persistentDataPath + "/" + editorImageToUpload;
        Debug.LogError("path: " + Application.persistentDataPath + "/" + "maintemp.PNG");
        hasPicked = true;

#elif UNITY_ANDROID
        Debug.LogError("Executing from Unity android");
        Debug.LogError("Starting to pick image");

        try 
        {
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( (path) =>
            {
                Debug.Log("path: " + path);
                if( path != null )
                {         
                    pickedImagePath =  path;
                    hasPicked = true;
                }
                else {
                    Debug.LogError("Null path when trying to select image from gallery");
                    hasCanceled =  true;
                }
            }, "Select a JPG image", "image/jpg", maxImageSize);

            Debug.Log( "Permission result: " + permission);
        }
        catch (System.Exception e) 
        {
            Debug.LogError("Error trying to select image from gallery");
            Debug.LogError(string.Format(" received error {0}", e.Message));
            throw;
        }
#endif
        while (!hasPicked) 
        {
            if (hasCanceled) 
            {
                yield break;
            }
            else 
            {
                yield return null;
            }
        }

        try 
        {
            AWSInterface.Instance.PostObject(pickedImagePath, SaveImageToProfile, (postedPath) => 
            {
                Debug.LogError("Error trying to update profile image from: " + pickedImagePath);
            });
        }
        catch (System.Exception e) 
        {
            Debug.LogError("Error trying to post image to bucket");
            Debug.LogError(string.Format(" received error {0}", e.Message));
            throw;
        }
    }

    void SaveImageToProfile(string path)
    {
        var imageUpdate = new UpdateAvatarUrlRequest()
        {
            ImageUrl = Path.GetFileName(path)
        };

        PlayFabClientAPI.UpdateAvatarUrl(imageUpdate, (response) => {
            DisplayPickedProfileImage(path, maxImageSize);
        }, PlayFabServices.OnPlayFabError);
    }

    void DisplayPickedProfileImage(string path, int maxSize)
    {
        // Create Texture from selected image
        Texture2D texture = NativeGallery.LoadImageAtPath( path, maxSize );
        if( texture == null )
        {
            Debug.Log( "Couldn't load texture from " + path );
            return;
        }

        var profilePic = Sprite.Create(texture, ((RectTransform)profileBTN.transform).rect, new Vector2(0.5f, 0.5f), 1.0f);
        profileBTN.image.sprite = profilePic;
        profileBTN.image.material = null;
    }

    void FollowProfile()
    {
        var followRequest = new AddFriendRequest()
        {
            FriendPlayFabId = displayedProfileId
        };

        PlayFabClientAPI.AddFriend(followRequest, (addResult) => {
            Debug.Log("Friend added");
        }, PlayFabServices.OnPlayFabError);
    }

    public void AddImage()
    {
        StartCoroutine(AddImageProcess());
    }

    IEnumerator AddImageProcess() 
    {
        string pickedImagePath = "";
        bool hasPicked = false;
        bool hasCanceled = false;

#if UNITY_EDITOR
        Debug.LogError("Executing from Unity editor");
        pickedImagePath = Application.persistentDataPath + "/" + editorImageToUpload;
        Debug.LogError("path: " + Application.persistentDataPath + "/" + "maintemp.PNG");
        hasPicked = true;

#elif UNITY_ANDROID
        Debug.LogError("Executing from Unity android");
        Debug.LogError("Starting to pick image");
        try 
        {
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( (path) => 
            {
                Debug.Log("path: " + path);
                if( path != null ) 
                {
                    pickedImagePath = path;
                    hasPicked = true;
                }
                else 
                {
                    Debug.LogError("Null path when trying to select image from gallery");
                    hasCanceled =  true;
                }
            }, "Select a PNG image", "image/png", maxImageSize);

            Debug.Log( "Permission result: " + permission);
        }
        catch (System.Exception e) 
        {
            Debug.LogError("Error trying to select image from gallery");
            Debug.LogError(string.Format(" received error {0}", e.Message));
            throw;
        }
#endif
        while (!hasPicked) 
        {
            if (hasCanceled) 
            {
                yield break;
            }
            else 
            {
                yield return null;
            }
        }

        // userInfoLoading.StartLoading();
        // userInfoLoading.UpdateLabel("Uploading image");

        PlayFabServices.Instance.GetUserData(new GetUserDataParameters()
        {
            userId = displayedProfileId,
            requiredData = new List<string>() {"UserImages"},
            succesCallback = (getResult) => 
            {
                Debug.Log("Got user data:");
                UserImageCollection images =  null;
                if (getResult.Data == null || !getResult.Data.ContainsKey("UserImages")) 
                {
                    Debug.Log("No UserImages");
                    images = new UserImageCollection();
                }
                else 
                {
                    var value = getResult.Data["UserImages"].Value;
                    Debug.Log(getResult.Data["UserImages"].Value);
                    images = JsonUtility.FromJson<UserImageCollection>(value);
                    // Codigo para añadir mas informacion al json
                }

                images.AddImageToCollection(Path.GetFileName(pickedImagePath));

                AWSInterface.Instance.PostObject(pickedImagePath, (postedPath) => 
                {
                    PlayFabServices.Instance.UpdateUserData(new UpdateUserDataParameters() 
                    {
                        data = new Dictionary<string, string>() 
                        {
                            {"UserImages", JsonUtility.ToJson(images)}
                        },
                        succesCallback = (updateResult) => 
                        {
                            Debug.Log(JsonUtility.ToJson(images));
                            Debug.Log("Successfully updated user images");
                        },
                        errorCallback = () => 
                        {
                            Debug.Log("Got error updating user data");
                        }
                    });
                }, (keyValue) => 
                {
                    Debug.LogError("Got error trying to post image to bucket");
                });
            }, errorCallback = () => 
            {
                Debug.LogError("Got Error trying to get user data");
            }
        });
    }
#endregion
}
