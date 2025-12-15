FROM unityci/editor:6000.1.15f1-windows-mono-3

WORKDIR /project
COPY . .

RUN unity-editor \
  -batchmode \
  -nographics \
  -quit \
  -projectPath /project \
  -buildTarget Win64 \
  -executeMethod BuildScript.BuildWindows \
  -logFile /project/build.log
