# FireBolt
#### A reimagining of temporally oriented cinematic realization


## What is this thing?
FireBolt is a system utilizing a collection of declarative representations for actions, characters, and camera directives and an algorithm that together choreograph actions and cause cameras to film them in a virtual environment.  

## Do I care about it?
As a cinematic realization system, FireBolt has many applications, including but not limited to:
+ Visualization of computer or human authored event plans
+ Generative tool for experiments on cognitive psychological effects of film techniques, event ordering, spatial arrangement, etc
+ Film Previsualization
+ Machinima

If you want to do something related to the words cinema and virtual, then FireBolt may be a good fit.

## How do I run it?
### Standalone
download a release & c
### Embedded
+ download the .unitypackage from the releases in this repo
+ add that package to your project
+ add the FireBolt.prefab at the origin of your scene
+ add a call to FireBolt.ElPresidente.Init()
  + there are several defaulted parameter values in this call, the exact nature of which will depend on the version of FireBolt used.  refer to the doc comments in the code for their use.
  + you may override the default file paths loaded by FireBolt by supplying the Init() method with a populated InputSet object.  otherwise the below defaults will be used as inputs to the system
    + story plan - storyPlans/defaultStory.xml
    + cinematic model - cinematicModels/defaultModel.xml
    + camera plan - cameraPlans/defaultCamera.xml
    + actor bundle - AssetBundles/actorsandanimations
    + terrain bundle - AssetBundles/terrain


## How does it work?
FireBolt is implemented as a Unity project that consumes .net framework 3.5 dlls which in turn define structure for input knowledge.  
#### Representation
There are 4 kinds of information that FireBolt needs in order to execute.
1. story plan representation - This is list of all the things that happen, templates for the kinds of things that can happen, and a list of the names of actors that things can happen with and to.  This representation is provided to FireBolt in the Impulse v1.336 format of which there are examples in the FireBolt releases.
2. cinematic model - Defines how the actions in the story plan should be shown in the engine and which models and animations should be used for the characters named in the story.  The representation is done conforming to the xml schema in the cinematic model project in this repository.
3. asset bundles - the files containing 3D models and animations that will be used for rendering within FireBolt are located in asset bundles.  Generally these will not be changed by the user.  Instead, the user would change the cinematic model assignment of which actor name should refer to which model that is in the bundle.  There is a default bundle attached to the release.
4. camera plan - how to view the unfolding action is determined by camera actions conforming to the xml schema of the Oshmirto language whose parser is contained in this repo.  The camera plan is a sequentially executed set of directives for where to place a camera, what to look at, and how to move in the environment.

#### Execution
When ElPresidente.Init() is called, FireBolt
1. All input files specified (or defaulted to) are checked to see if they have been updated since last Init() call then they are loaded in the following order
   1. Story plan
   2. Cinematic model
   3. Camera plan
   4. Asset bundles
  2. The story plan and cinematic model are used to build engine-actionable commands (think command pattern) that FireBolt can operate over throughout its execution.  In general, commands requiring additional information, such as reference to an exact model file will be looked up as needed (when the command first executes) and saved into the command at that time.  These commands are placed onto the actor queue.
  3. The camera plan is used to generate similar commands for the engine, but these all pertain either to time control (what time in the story should we see right now, allowing for flashbacks and the like) and are appended to a discourse queue or they pertain to  attributes of the camera used to view the action and are appended to a camera queue.
  4. Initialization done, at each frame FireBolt executes all the actions that should have happened since the last frame from each of the queues in this order: 
     1. discourse queue - move the story to the correct time for what we want to film now
     2. actor queue - make the actors do what they're supposed to do right now
     3. camera queue - put the camera where it should be once the action is settled

5. Execution continues in this fashion until Init() is called again (and changed files are reloaded causing the whole business starts from the beginning) or the application is exited



## No, No, how does it _really_ work?
There is an as yet [unpublished paper](https://github.com/LAS-NCSU/NP-FireBolt/blob/master/docs/Firebolt_Overview_Paper.pdf) detailing the formalism of the system for those with a lot of curiosity and a love of rigor.
