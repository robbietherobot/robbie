using RobbieSenses.Interfaces;
using RobbieSenses.Output;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using RobbieSenses.Input;
using RobbieSenses.Evaluation;
using RobbieSpinalCord;
using Microsoft.Cognitive.LUIS;
using RobbieSenses.Devices;
using RobbieSenses.Intents;
using RobbieSpinalCord.Interfaces;
using System;
using RobbieSpinalCord.Models;
using System.Collections.Generic;
using RobbieSenses.Actions;

namespace RobbieSenses
{
    /// <summary>
    /// Brain class handling the coordination of all senses.
    /// </summary>
    public class Brain : IBrain
    {
        /// <summary>
        /// String identifying unidentified users interacting with Robbie.
        /// </summary>
        private const string AnonymousPersonId = "Anonymous";

        /// <summary>
        /// Event handler to subscribe to to get informed about all events triggered by Robbie's brains.
        /// </summary>
        public event SenseEventHandler SenseEvent;

        /// <summary>
        /// UtterancePrediction object used to get the intent of recognized speech to text.
        /// </summary>
        private readonly UtterancePrediction utterancePrediction;        

        /// <summary>
        /// The Ears object controlling the ears input.
        /// </summary>
        private readonly IEars ears;       

        /// <summary>
        /// The Eyes object controlling the eyes and vision.
        /// </summary>
        private readonly Eyes eyes;        

        /// <summary>
        /// The Voice object controlling the voice output.
        /// </summary>
        private readonly IVoice voice;

        /// <summary>
        /// Robbie's sleeping state, keeping track if Robbie is sleeping or awake.
        /// </summary>
        private bool sleeping;

        /// <summary>
        /// The ClientConnectionPool object used to manage all server connections.
        /// </summary>
        private readonly ClientConnectionPool pool;

        /// <summary>
        /// The person ID of the currently identified face (person) to interact with.
        /// </summary>
        private string currentIdentityPersonId;

        /// <summary>
        /// Constructs a new brain object.
        /// </summary>
        /// <param name="visionPreview">A capture element that is placed on a canvas used for capturing what Robbie sees.</param>
        /// <param name="previewCanvas">A canvas element used for rendering the image preview showing what Robbie sees.</param>
        /// <param name="audioPlaybackElement">A media element that is placed on a canvas used for speaking.</param>
        public Brain(CaptureElement visionPreview, Canvas previewCanvas, MediaElement audioPlaybackElement)
        {
            utterancePrediction = new UtterancePrediction();
                  
            ears = new Ears();
            voice = new Voice(audioPlaybackElement);
            eyes = new Eyes(visionPreview, previewCanvas);

            pool = new ClientConnectionPool();
            
            currentIdentityPersonId = AnonymousPersonId;
            
            // define the event handlers for handling the different events occuring from all the senses            
            ears.SpeechRecognized += Ears_SpeechRecognized;
            ears.EarsStateChanged += Ears_EarsStateChanged;
            voice.FinishedPlaybackEventHandler += Voice_FinishedPlayback;            
            eyes.NewActivePersonEvent += Eyes_NewActivePersonEvent;

            // always start in a sleeping state (not actively listening)
            sleeping = true;
        }

        /// <summary>
        /// Stores the current image capture containing the face of a person for training purposes.
        /// </summary>
        /// <param name="name">The name of the person to store the face for.</param>
        public async void StoreFaceFor(string name)
        {
            await eyes.StoreFaceFor(name);
        }

        /// <summary>
        /// Trains the person group, which should be called after storing new faces or new face data.
        /// </summary>
        /// <returns>A Task object for this is an asynchronous method.</returns>
        public async Task TrainFaces()
        {
            await eyes.TrainFaces();
        }

        /// <summary>
        /// Respond to ears state changed event, reporting the event to the UI / logging.
        /// </summary>
        /// <param name="state">The new state the ears are in.</param>
        private void Ears_EarsStateChanged(EarsState state)
        {
            ReportEvent("ears", $"state to '{state}'");
        }

        /// <summary>
        /// When a new active person has been identified, this event gets triggered.
        /// </summary>
        /// <param name="identity">The TrackedIdentity of the new person to interact with.</param>
        private async void Eyes_NewActivePersonEvent(TrackedIdentity identity)
        {
            // only continue if we have an identity
            if (identity == null) return;

            // if there is a personID, the person was identified by face recognition
            if (identity.PersonId != Guid.Empty)
            {
                ReportEvent("brain", $"sensed a new person with ID {identity.PersonId}");

                // sets the current identity, which can be used by brains
                currentIdentityPersonId = identity.PersonId.ToString();
                var client = GetClientForCurrentUser();

                // identify the current user with sitecore
                // this will return the analytics cookie and all activities will be logged to the right identity in XDB
                var response = await client.Identify();

                // if the gender isn't set within the identified profile, get the gender from the Face API appereance and update the profile
                if (string.IsNullOrEmpty(response.Gender))
                {
                    var p = new Profile
                    {
                        Gender = identity.Appearance.Gender
                    };
                    await client.UpdateProfile(p);
                }

                // if there is no name known yet, ask the name of the person (should be triggered by engagement plan? probably in separate action)
                if (string.IsNullOrEmpty(response.Name))
                {
                    await AskForName();
                }

                // else if the name is known, greet the person and get the identify intent action (should be renamed)               
                else
                {
                    
                    var intentAction = await client.GetIntentAction("identify");
                    var text = intentAction.Reply.Replace("{name}", response.Name);
                    var emotion = intentAction.Emotion;

                    await SayTextWithEmotion(text, emotion);                    
                }
            }

            // if unknown (not identified)
            else if (identity.PersonId == Guid.Empty)
            {
                // set anonymous ID and identify to Sitecore without a name
                currentIdentityPersonId = AnonymousPersonId;
                var client = GetClientForCurrentUser();
                await client.Identify();

                if (identity.Appearance != null)
                {
                    var p = new Profile
                    {
                        Gender = identity.Appearance.Gender
                    };
                    await client.UpdateProfile(p);
                }

                // ask name (engagement, trigger action to ask name) - no other actions should occur
                await AskForName();
            }
        }

        /// <summary>
        /// Ask for the user's name.
        /// </summary>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task AskForName()
        {
            var text = "What's your name?";
            var emotion = "Disgust";
            await SayTextWithEmotion(text, emotion);
        }

        /// <summary>
        /// Used when Robbie wants to say something, but doesn't react on a voice command.
        /// </summary>
        /// <param name="text">The text to say.</param>
        /// <param name="emotion">The emotion to show.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task SayTextWithEmotion(string text, string emotion)
        {
            var sayAction = new SayAction(text);
            var emotionAction = new EmotionAction(emotion);

            var actions = new List<IAction> {emotionAction, sayAction};

            await ExecuteActions(actions);
        }

        /// <summary>
        /// Wakes up Robbie's brains and starts all relevant senses.
        /// </summary>
        public void WakeUp()
        {
            sleeping = false;
            eyes.WakeUp();
            ears.StartListening();
            ReportEvent("brain", "waking up!");
        }

        /// <summary>
        /// Hibernate Robbie's brains, but keep listening, waiting for the wake up command.
        /// </summary>
        public void Hibernate()
        {
            sleeping = true;
            eyes.Hibernate();
            ears.StartListening(); // but do keep listening
            ReportEvent("brain", "hibernating...");
        }

        /// <summary>
        /// Callback hanlder of speech recognized event of ears, called when an utterance is recognized.
        /// This event handler is used to feed the utterance prediction via LUIS.
        /// </summary>
        /// <param name="utterance">The utterance recognized by the ears.</param>
        private async void Ears_SpeechRecognized(string utterance)
        {
            // stop listening when starting to process the recognized utterance
            ears.StopListening();
            ReportEvent("ears", $"heared '{utterance}'");

            // if the speech recognized turns out to be an empty string, just continue listening
            if (string.IsNullOrEmpty(utterance))
            {
                ears.StartListening();
                return;
            }

            // get intent from utterance
            var intent = await Utterance(utterance);           

            // handle intent logic                
            var actions = await HandleIntent(intent);

            // execute the actions
            await ExecuteActions(actions);
  
            // if in a sleeping state, make sure we do keep listening
            if (sleeping)
            {
                ears.StartListening();
            }
        }

        /// <summary>
        /// Handle the utterance found with speech to text recognition.
        /// </summary>
        /// <param name="text">The utterance.</param>
        private async Task<LuisResult> Utterance(string text)
        {
            LuisResult intent = null;
            if (!string.IsNullOrEmpty(text))
            {
                intent = await utterancePrediction.GetIntent(text);
            }

            // and start listening again            
            return intent;
        }

        /// <summary>
        /// Handles the intent logic.
        /// </summary>
        /// <param name="intent">The LuisResult containing the detected intent.</param>
        /// <returns>A list of actions for Robbie to execute, based on the provided intent.</returns>
        private async Task<IList<IAction>> HandleIntent(LuisResult intent)
        {
            // todo: UpdateEmotions shouldn't always happen. Preferably move to Intent.HandleIntent or in it's actionclass
            if (!currentIdentityPersonId.Equals(AnonymousPersonId))
            {
                await UpdateEmotion();
                var experienceProfile = await GetClientForCurrentUser().GetExperienceProfile();

                if (experienceProfile.OnsiteBehavior.ActiveProfiles?.Length > 0 && experienceProfile.OnsiteBehavior.ActiveProfiles[0].PatternMatches?.Length > 0)
                {
                    // todo: log profile card to events, just debugging purposes
                    var patternName = experienceProfile.OnsiteBehavior.ActiveProfiles?[0].PatternMatches?[0].PatternName;
                    var matchPercentage = experienceProfile.OnsiteBehavior.ActiveProfiles?[0].PatternMatches?[0].MatchPercentage;
                    ReportEvent("behaviour", $"{patternName} {matchPercentage}");

                    // todo: get profile, just for debugging purposes
                    // ReSharper disable once UnusedVariable
                    var profile = await GetClientForCurrentUser().GetProfile();
                }
            }
            
            // create intent handler after intent has been returned and retrieve all actions that are tied to this intent
            var handler = new IntentHandler(intent, GetClientForCurrentUser());
            var actions = await handler.HandleIntent();

            return actions;
        }

        /// <summary>
        /// Executes all given actions in order of occurance.
        /// </summary>
        /// <param name="actions">The actions to be executed.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task ExecuteActions(IEnumerable<IAction> actions)
        {
            // using the dynamic visitor pattern
            // the correct overload will automatically be used
            foreach(var action in actions)
            {
                await this.Execute((dynamic)action);
            }
        }

        /// <summary>
        /// Says something.
        /// </summary>
        /// <param name="action">The say action containing the text to say.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task Execute(SayAction action)
        {
            if (!sleeping)
            {
                var reply = action.Reply;

                // if an identity is currently tracked, replace the name in the response, if applicable
                if (reply.Contains("{name}"))
                {
                    if (!string.IsNullOrEmpty(eyes.CurrentIdentityName))
                    {
                        var name = eyes.CurrentIdentityName;
                        if (name.Contains("-"))
                        {
                            name = name.Substring(0, name.IndexOf('-'));
                        }
                        reply = reply.Replace("{name}", name);
                    }
                    else
                    {
                        reply = reply.Replace("{name}", string.Empty);
                    }
                }

                await Say(reply);
            }
        }

        /// <summary>
        /// Identifies the current user.
        /// </summary>
        /// <param name="action">The identify action containing the person ID of the user.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task Execute(IdentifyAction action)
        {
            if (!sleeping)
            {
                await SetIdentity(action.PersonId);
            }
        }

        /// <summary>
        /// Executes the action for when a person has been named.
        /// </summary>
        /// <param name="action">The name action containing the name of the user.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task Execute(NameAction action)
        {
            if (sleeping) return;

            if(GetClientForCurrentUser().PersonId.Equals(AnonymousPersonId, StringComparison.OrdinalIgnoreCase))
            {
                var personId = await eyes.CreatePerson();
                currentIdentityPersonId = personId.ToString();
                pool.ChangeId(AnonymousPersonId, currentIdentityPersonId);                                

                var newName = await eyes.UpdatePerson(personId, action.Name);
                await eyes.StoreFaceFor(newName);
            }

            var p = new Profile
            {
                Name = action.Name
            };
            await GetClientForCurrentUser().UpdateProfile(p);

            await voice.Say($"Hi {action.Name}, nice to meet you!");            
        }
        
        /// <summary>
        /// Lets Robbie show a certain emotion.
        /// </summary>
        /// <param name="action">The emotion action containing the emotion to show.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
#pragma warning disable 1998
        private async Task Execute(EmotionAction action)
#pragma warning restore 1998
        {
            if (sleeping) return;

            eyes.ShowEmotion(action.Emotion);
            ReportEvent("eyes", $"show emotion '{action.Emotion}'");
        }

        /// <summary>
        /// Executes a pre-configured command.
        /// </summary>
        /// <param name="action">The command action containing the command to execute.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
#pragma warning disable 1998
        private async Task Execute(CommandAction action)
#pragma warning restore 1998
        {
            // if the hibernate command is given, put Robbie to sleep
            if (action.Command.Equals(UtterancePrediction.HibernateCommand))
            {
                Hibernate();
            }
            
            // if wake up command is given, wake up Robbie again
            if (action.Command.Equals(UtterancePrediction.WakeUpCommand))
            {
                WakeUp();
            }
            
            // if quit command is given, report the quit command as an event, so it can be picked up by the application to dispose itself
            if (action.Command.Equals(UtterancePrediction.QuitCommand))
            {
                ReportEvent("brain", UtterancePrediction.QuitCommand);
            }                                     
        }

        /// <summary>
        /// Empty execution method, used when no intent has been detected, to make sure to start listening again when no other action has been started.
        /// </summary>
        /// <param name="action">The (empty) no intent action.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
#pragma warning disable 1998
        // ReSharper disable once UnusedParameter.Local because the parameter is required for the pattern, but not relevant here
        private async Task Execute(NoIntentAction action)
#pragma warning restore 1998
        {
            ears?.StartListening();
        }

        /// <summary>
        /// Sends the latest emotion score to the server as a profile card, updating the emotion profile on the server.
        /// </summary>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task UpdateEmotion()
        {
            var emotions = await eyes.GetEmotions();
            if (emotions == null) return;

            var emotionScore = new ProfileCardViewModel
            {
                Anger = emotions.Anger * 100,
                Contempt = emotions.Contempt * 100,
                Disgust = emotions.Disgust * 100,
                Fear = emotions.Fear * 100,
                Happiness = emotions.Happiness * 100,
                Neutral = emotions.Neutral * 100,
                Sadness = emotions.Sadness * 100,
                Surprise = emotions.Surprise * 100
            };

            await GetClientForCurrentUser().UpdateProfileEmotions(emotionScore);
        }

        /// <summary>
        /// Gets the connection pool client object for the current user (the person we're interacting with).
        /// </summary>
        /// <returns>A connection pool object of the current user.</returns>
        private IClient GetClientForCurrentUser()
        {
            return pool.GetClient(currentIdentityPersonId);
        }

        /// <summary>
        /// Callback handler of playback end event of voice, called when finished talking.
        /// This event handler is used for the ears to start listening again after the voice has finished its say method.
        /// </summary>
        private void Voice_FinishedPlayback()
        {
            // when voice has stopped, we can start listening again!
            ears?.StartListening();
        }        

        /// <summary>
        /// Tell the voice to say the given text, while pausing the listening process to prevent Robbie to interpret his own speech.
        /// </summary>
        /// <param name="text">The text to say.</param>
        public async Task Say(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            ears.StopListening();

            await voice.Say(text);            
            ReportEvent("voice", $"say '{text}'");            
        } 

        /// <summary>
        /// Sets the identity to interact with.
        /// </summary>
        /// <param name="personId">The Person ID from the Face API, uniquely identifying a person and profile.</param>
        /// <returns>Returns a task object for this method is asynchronous.</returns>
        public async Task SetIdentity(string personId)
        {
            // if the person ID isn't empty (hence: new unidentified person) and the current identity is already set to the same value
            // return, for it is not necessary to switch the connection client when in fact, nothing changed
            if (!personId.Equals(Guid.Empty.ToString()) && currentIdentityPersonId.Equals(personId))
            {
                return;
            }

            currentIdentityPersonId = personId;
            var client = pool.GetClient(currentIdentityPersonId);

            // identify the current person at Sitecore, returning a profile
            var response = await client.Identify();

            // create a new profile based on the returned profile data
            var p = new Profile()
            {
                Name = response.Name,
                BirthDate = response.BirthDate,
                Gender = response.Gender
            };

            // merge profile with current profile from Robbie
            var mergedProfile = MergeProfile(p);

            // send merged profile to Sitecore, updated properties will be synced
            await client.UpdateProfile(mergedProfile);            

            // get action text from the server
            var action = await client.GetIntentAction("identify");                            
            var text = action.Reply.Replace("{name}", mergedProfile.Name);
            var emotion = action.Emotion;
            await SayTextWithEmotion(text, emotion);
        }

        /// <summary>
        /// Merges the given profile with the currently tracked profile by Robbie.
        /// </summary>
        /// <param name="p">The profile to merge into the currently tracked profile.</param>
        /// <returns>The merged profile.</returns>
        private Profile MergeProfile(Profile p)
        {
            // todo: not implemented yet!
            return p;
        }

        /// <summary>
        /// Event handler passing through triggered events to hanlders subscribed to the SenseEvent.
        /// </summary>
        /// <param name="sense">The sense that fired the event.</param>
        /// <param name="message">A message describing the event.</param>
        private void ReportEvent(string sense, string message)
        {
            var handler = SenseEvent;
            // ReSharper disable once UseNullPropagation
            if (handler != null)
            {
                handler(sense, message);
            }
        }

        /// <summary>
        /// Disposes all disposable members: releases the camera, turns of the LEDs and centers all servos. 
        /// </summary>
        public void Dispose()
        {
            Camera.Instance.Dispose();
            ServoHat.Instance.Dispose();
            eyes.Dispose();
        }
    }
}
