Add the following lines to your manifest to include this package and its dependencies:
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unidice.sdk": "https://github.com/ExtraNiceBV/Unidice-SDK.git",

To be able to build with this:
Setup:
- make sure your project is using at least Unity 2020.3. Better use Unity 2021.3, because building for Android with 2020.3 is a nightmare.
- make sure Java jdk 1.8.0_311 is installed and set up to be used in Preferences > External Tools
- install Google's Unity JAR Resolver (https://github.com/googlesamples/unity-jar-resolver/tags). It's required to download all the dependencies.
- in the Player Settings set the target SDK API Level to 30 and Minimum API Level to 24
- in the Player Settings, check Minify > Use R8