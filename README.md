# Robbie the Sitecore robot #

Robbie is a robot that moves, interacts, communicates and executes tasks, fully driven by Sitecore XP, using additional techniques like Artificial Intelligence, Machine Learning, Natural Language Processing, Face Recognition & Emotion Detection powered by the Microsoft Cognitive Services API's.

## Why? ##

Because it's cool. And to showcase the power of Sitecore XP and experiment with profiling and interaction.

## Installation ##

### Setup the server application ###

* Place vanilla webroot files of Sitecore 8.2 Update 1 in "server/website" folder
* Place data folder contents and license.xml file in "server/data" folder
* Attach the Sitecore databases, you can place the LDF and MDF files in the corresponding folder within "server/databases", they will be ignored by git though
* Sitecore Kernel, MVC and other assemblies are referenced via NuGet, so you don't need to add them manually to the solution
* Create a new IIS site named "Robbie"
* Add a hosts entry for "robbie.dev" pointing to your localhost
* Create your own ConnectionStrings.Development.config and DataFolder.Development.config translation files (included in project, but not in repository)
* You should now be able to run the Sitecore server application for Robbie
* Synchronize your vanilla database by going to "http://robbie.dev/unicorn.aspx" and clicking the "Sync" button
* Furthermore, there's nothing really in here... just vanilla Sitecore install with some profile key changes

### Initial setup of client application ###

* Make sure you enable the speech to text feature of Windows 10 before you start the application when running on your pc: "Speech Settings" > "Speech, Inking & typing privacy settings" > "Get to know me" > "Turn on"
* Please note: due to limited hardware capabilities of the Pi, the client application runs way smoother on a pc than on a Raspberry Pi
* You can introduce yourself to Robbie via speech, but once recognized, you can copy in your personal Face ID into the textbox on the right to train yourself multiple times, to enhance recognition in the future 

### More info ###

See INSTALL.md for all details regarding the software and libraries used, including their versions, configuration and other setup info.

## Contact ##

If you have any questions, ideas or suggestions, please contact us via Twitter: [@rhabraken](https://twitter.com/rhabraken) and [@baslijten](https://twitter.com/baslijten).