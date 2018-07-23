using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace RobbieSenses.Evaluation
{
    /// <summary>
    /// Class implementing the actual face detection and recognition functionality based on the Microsoft Cognitive Services Face API.
    /// </summary>
    public class FaceDetection
    {
        /// <summary>
        /// The actual Microsoft Face API client.
        /// </summary>
        private readonly FaceServiceClient faceServiceClient;

        /// <summary>
        /// The person group ID defined in the face service client for the configured API key.
        /// </summary>
        private string personGroupId;

        /// <summary>
        /// Initializes the face detection based on the application resources configuration.
        /// </summary>
        public FaceDetection()
        {
            var resources = ResourceLoader.GetForCurrentView("/RobbieSenses/Resources");
            var faceApiKey = resources.GetString("FaceAPIKey");
            var groupId = resources.GetString("PersonGroupID");
            var groupName = resources.GetString("PersonGroupName");
            var groupMembers = resources.GetString("PersonGroupMembers");

            faceServiceClient = new FaceServiceClient(faceApiKey);

            SetGroup(groupId, groupName, groupMembers);
        }

        /// <summary>
        /// Gets the configured person group from the Face API or creates one if it doesn't exist already.
        /// </summary>
        /// <param name="groupId">The ID of the group to create or activate.</param>
        /// <param name="groupName">The name of the group to create or activate.</param>
        /// <param name="groupMembers">The pipe separated list of group members to initialize this group with.</param>
        private async void SetGroup(string groupId, string groupName, string groupMembers)
        {
            // store the group locally from now on
            personGroupId = groupId;

            // get the group from the api service
            var groups = await faceServiceClient.ListPersonGroupsAsync();
            var group = groups.FirstOrDefault(g => g.PersonGroupId.Equals(groupId, StringComparison.CurrentCultureIgnoreCase));

            // when the group isn't null, we're finished
            if (group != null) return;

            // but if the group doesn't exist, create the group
            await faceServiceClient.CreatePersonGroupAsync(groupId, groupName);

            // and add the default members if supplied
            if (!string.IsNullOrEmpty(groupMembers))
            {
                foreach (var member in groupMembers.Split('|'))
                {
                    await CreatePerson(member);
                }
            }
        }

        /// <summary>
        /// This method creates a new person if it doesn't exist yet within the Face API, and uses its newly created person ID as its name.
        /// </summary>
        /// <param name="name">The name of the person to create: can either be a real name when updating a person, or a temporary GUID for a newly found person.</param>
        /// <returns>The final person ID of the Face API created person, or the ID of the person with this name that already existed.</returns>
        public async Task<Guid> CreatePerson(string name)
        {
            var persons = await faceServiceClient.ListPersonsAsync(personGroupId);

            var result = persons.Where(p => p.Name == name).ToList();

            if (result.Any())
            {
                // if the person exists, update its name with the possible new value
                var person = result.FirstOrDefault();
                return person.PersonId;
            }

            // if the person doesn't exist, create him or her
            var createPersonResult = await faceServiceClient.CreatePersonAsync(personGroupId, name);

            // now update the name with its Face API person ID (because we do not yet know the name of newly found persons)
            var newName = createPersonResult.PersonId.ToString();
            await faceServiceClient.UpdatePersonAsync(personGroupId, createPersonResult.PersonId, newName);
            return createPersonResult.PersonId;
        }

        /// <summary>
        /// Stores the image containing the face of the given person for training purposes.
        /// </summary>
        /// <param name="name">The name of the person whose face is in the given image.</param>
        /// <param name="imageStream">The stream of the image containing the face of the given person.</param>
        /// <returns>A boolean indicating whether or not the storing succeeded.</returns>
        public async Task<bool> StoreFaceFor(string name, Stream imageStream)
        {                        
            var persons = await faceServiceClient.ListPersonsAsync(personGroupId);            
            foreach (var person in persons)
            {
                if (person.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    await faceServiceClient.AddPersonFaceAsync(personGroupId, person.PersonId, imageStream);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Trains the person group, which should be called after storing new faces or new face data.
        /// </summary>
        public async Task TrainFaces()
        {
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);

            // wait for the training to finish  
            while (true)
            {
                var trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Renames a person to concatenating his person ID to his name (while previously probably only being named by his ID).
        /// </summary>
        /// <param name="personId">The personId object of the Face API.</param>
        /// <param name="name">The actual name of the person.</param>
        /// <returns>Returns the new name of the person.</returns>
        public async Task<string> UpdatePersonName(Guid personId, string name)
        {
            var newName = $"{name}-{personId}";
            await faceServiceClient.UpdatePersonAsync(personGroupId, personId, newName);
            return newName;
        }

        /// <summary>
        /// Detects faces, without recognizing / identifying them yet - only detects the presence of all faces within the given image stream.
        /// </summary>
        /// <param name="imageStream">The stream of the image to detect the faces in.</param>
        /// <returns>An array of Face objects, describing the faces found (ID and bounding box).</returns>
        public async Task<Face[]> DetectFaces(Stream imageStream)
        {
            var attrs = new List<FaceAttributeType>
            {
                FaceAttributeType.Age,
                FaceAttributeType.FacialHair,
                FaceAttributeType.Glasses,
                FaceAttributeType.Gender,
                FaceAttributeType.Emotion
            };
            var faces = await faceServiceClient.DetectAsync(imageStream, returnFaceAttributes: attrs);

            return faces;
        }

        /// <summary>
        /// Tries to identify one or more of the given faces.
        /// </summary>
        /// <param name="faces">A list of already detected faces.</param>
        public async Task<List<Person>> IdentifyFace(Face[] faces)
        {            
            var faceIds = faces.Select(face => face.FaceId).ToArray();
            var identifiedPersons = new List<Person>();
            var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);

            foreach (var identifyResult in results)
            {
                if (identifyResult.Candidates.Length > 0)
                {
                    var candidateId = identifyResult.Candidates[0].PersonId;
                    var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);                                                
                    identifiedPersons.Add(person);
                }
            }

            return identifiedPersons;
        }
    }
}
