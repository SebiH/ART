# ART - Augmented Reality above the Tabletop

![Augmented Reality above the Tabletop](/art.jpg?raw=true "Augmented Reality above the Tabletop")

## Intro

ART is an immersive analysis tool for the visual analysis of mobile health data. It visualises multidimensional data in augmented reality using an interactive 3D visualisation. The visualisation links related data points between several 2D scatter plots to create a 3D parallel coordinates visualisation. To benefit from well-established interaction techniques, the visualisation is anchored to a touch-sensitive tabletop. 

This system is part of my Bachelor Thesis in Computer Science (B.Sc.) at the University of Konstanz, Germany.

The system is split into four separate applications:

* A **web application** (TypeScript + Angular) for controlling the visualisation
* A **Unity3D** (C#) application for creating a visualisation in augmented reality
* A **web server** (TypeScript + Node.js) for communication between the web application and Unity3D
* A **library** (C++11) for handling and processing camera images

In addition, there are several helper tools, e.g. for calibrating cameras, debugging, and OptiTrack tracking.


## Layout

    art_code/
    │
    ├─ data/                            camera calibration data
    │                                   (e.g. for ARToolKit 5)
    │
    ├─ interactivedisplay/
    │  │
    │  ├─ client/                       web application
    │  │
    │  └─ server/                       web server
    │
    ├─ tools/                           custom C++ tools and libraries
    │  │
    │  ├─ ArToolkitCalibration/         modified artoolkit sample for
    │  │                                calibrating OvrVision cameras
    │  │
    │  ├─ GUI/                          graphical interface for quickly
    |  |                                debugging the image processing library
    │  │                                and calibrating cameras
    │  │
    │  ├─ ImageProcessing/              image processing library responsible for
    │  │                                fetching images, ARToolKit 5 marker
    │  │                                detection, and forwarding everything to
    │  │                                Unity3D
    │  │
    │  ├─ optitrack/                    server for parsing OptiTrack data and
    │  │                                forwarding everything to 
    │  │
    │  ├─ scripts/                      helper scripts for managing build files
    │  │
    │  └─ thirdparty/                   third party header and library files
    │
    └─ unity/                           unity3D project
       │
       └─ Assets/                       unit3d source files
          │
          ├─ Deprecated/                outdated code, no longer in use
          │
          ├─ Modules/                   main unity code, organised in several
          │                             submodules, each with their own code,
          │                             shaders, textures, scenes, etc.
          │
          ├─ Plugins/                   custom image processing library and
          │                             dependencies are copied here after build
          │
          ├─ Scenes/                    main scenes
          │
          └─ Modules/                   third party unity3d modules
