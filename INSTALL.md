
# Robbie the Sitecore robot - Install manual & dev log #

## List of technologies, tools, services & hardware ##

* Sitecore 8.2 Update 2
* mongoDB 3.2.x (MMAPv1)
* mLab Sandbox
* Unicorn 3.3.2
* Microsoft Visual Studio Community 2015
* SQL Server Express 2016
* .NET Framework 4.5.2 (for server)
* .NET Core Universal Windows Platform 5.1.0 (for client)
* SlowCheetah 2.5.15
* Microsoft Cognitive Services Face API
* Microsoft Cognitive Services Vision API
* Microsoft Cognitive Services Emotion API
* Language Understanding Intelligent Service (LUIS) - Beta
* Windows 10 IoT Core
* Windows Media FaceAnalysis (on device face detection and tracking (inbetween API calls))
* Windows Media SpeechSynthesis (text to speech)
* Windows Media SpeechRecognition (speech to text)
* Raspberry Pi 3 Model B
* Microsoft LifeCam HD-3000
* Trust Leto 2.0 USB Speakerset
* Adafruit 16-Channel PWM / Servo HAT for Raspberry Pi
* Adafruit Mini 8x8 LED Matrix w/I2C Backpack
* Servo's to be determined

## Configure startup app on Pi ##

* iotstartup list
* iotstartup add headed robbie
* shutdown /r /t 0

Switch back to default:

* iotstartup add headed IoTCoreDefaultApp

## Remarks ##

* If you're building for Raspberry Pi 3 select ARM.
* Tools for IoT + Emulator etc. not included in instructions, just follow VS instructions on the fly.
* Don't forget to go to your "Speech" settings in Windows, click on "Speech, inking & typing privacy settings" and click the Start getting to know me button.

## Examples and tutorials used ##

* https://www.microsoft.com/cognitive-services/en-us/face-api/documentation/face-api-how-to-topics/HowtoAnalyzeVideo_Face
* https://www.microsoft.com/cognitive-services/en-us/face-api/documentation/face-api-how-to-topics/HowtoIdentifyFacesinImage
* https://msdn.microsoft.com/en-us/windows/uwp/audio-video-camera/detect-and-track-faces-in-an-image
* https://github.com/Microsoft/Cognitive-Samples-VideoFrameAnalysis/
* https://developer.microsoft.com/en-us/windows/iot/samples/webcamapp
* https://developer.microsoft.com/en-us/windows/iot/samples/helloworld

## Sitecore ##

* Installed Sitecore Experience Platform 8.2 rev. 161221 (8.2 Update-2)
* Instance name: Robbie
* License: your own Sitecore (partner) license
* Local binding: robbie.dev
* Server binding: www.robbie.net
* Local UpStream binding: robbie.vision
* Server UpStream binding: vision.robbie.net

After installing, please create the following files (they are referenced in the .csproj file but not in the repository, because they are developer machine specific):

* ~\robbie\server\solution\RobbieBrains\App_Config\Include\DataFolder.Development.config
* ~\robbie\server\solution\RobbieBrains\App_Config\ConnectionStrings.Development.config

## mongoDB ##

Created the following deployments in your MongoDB deployment, for instance via mLab,
where you could use a Single-node Standard Line Sandbox (shared, 0.5 GB), plans located in the Amazon's EU (Ireland) Region (eu-west-1):

* analytics-dev
* tracking_live-dev
* tracking_history-dev
* tracking_contact-dev
* analytics-prod
* tracking_live-prod
* tracking_history-prod
* tracking_contact-prod

## Raspberry Pi 3 OS ##

Install Windows 10 IoT Core on the Raspberry Pi using the following tutorial:

* https://developer.microsoft.com/nl-nl/windows/iot/Downloads.htm
